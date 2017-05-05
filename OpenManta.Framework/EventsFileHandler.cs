using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using log4net;
using OpenManta.Core;

namespace OpenManta.Framework
{
	internal class EventsFileHandler : IEventsFileHandler, IStopRequired
	{
		/// <summary>
		/// Will be set to true when MantaMTA is stopping.
		/// </summary>
		private volatile bool _IsStopping;

		private static object _Lock = new object();

		/// <summary>
		/// If true then a Task is already running for processing bounced emails.
		/// </summary>
		private static bool _BounceProcessingRunning = false;

		/// <summary>
		/// If true then a Task is already running for processing of feedback loops.
		/// </summary>
		private static bool _AbuseProcessingRunning = false;

		/// <summary>
		/// This is the name of the subdirectory where bounce/fbl emails that can't
		/// be processed are placed.
		/// </summary>
		private const string _SubdirectoryForProblemEmails = "UnableToProcess";

		/// <summary>
		/// This is the name of the subdirectory where bounce emails are placed when the MtaParameters.KeepBounceFiles flag is set to true.
		/// </summary>
		private const string _SubdirectoryForProcessedBounceEmails = "SuccessfullyProcessed";

		private readonly IEventHttpForwarder _events;
		private readonly ILog _logging;
		private readonly IEventsManager _eventsManager;
		private readonly IMtaParameters _config;

		public EventsFileHandler(IEventHttpForwarder events, IMantaCoreEvents coreEvents, log4net.ILog logging, IEventsManager eventsManager, IMtaParameters config)
		{
			Guard.NotNull(events, nameof(events));
			Guard.NotNull(coreEvents, nameof(coreEvents));
			Guard.NotNull(logging, nameof(logging));
			Guard.NotNull(eventsManager, nameof(eventsManager));
			Guard.NotNull(config, nameof(config));

			_events = events;
			_logging = logging;
			_eventsManager = eventsManager;
			_config = config;

			_IsStopping = false;

			// EventsFileHandler needs to be stopped when MantaMTA is stopping.
			coreEvents.RegisterStopRequiredInstance(this);

			// Make sure the drop folders exist.
			Directory.CreateDirectory(_config.BounceDropFolder);
			Directory.CreateDirectory(Path.Combine(_config.BounceDropFolder, _SubdirectoryForProblemEmails));
			Directory.CreateDirectory(_config.FeedbackLoopDropFolder);
			Directory.CreateDirectory(Path.Combine(_config.FeedbackLoopDropFolder, _SubdirectoryForProblemEmails));

			// Setup and start the bounce email file watcher.
			FileSystemWatcher bounceWatcher = new FileSystemWatcher(_config.BounceDropFolder, "*.eml");
			bounceWatcher.Created += DoBounceFileProcessing;
			bounceWatcher.EnableRaisingEvents = true;

			// Setup and start the feedback loop email file watcher.
			FileSystemWatcher abuseWatcher = new FileSystemWatcher(_config.FeedbackLoopDropFolder, "*.eml");
			abuseWatcher.Created += DoAbuseFileProcessing;
			abuseWatcher.EnableRaisingEvents = true;

			Thread t = new Thread(new ThreadStart(delegate ()
			{
				DoBounceFileProcessing(bounceWatcher, new FileSystemEventArgs(WatcherChangeTypes.All, _config.BounceDropFolder, string.Empty));
				DoAbuseFileProcessing(abuseWatcher, new FileSystemEventArgs(WatcherChangeTypes.All, _config.FeedbackLoopDropFolder, string.Empty));
			}));
			t.Start();
		}

		/// <summary>
		/// Method will be called to stop EventsFileHandler.
		/// </summary>
		public void Stop()
		{
			// Set is stopping to true and wait for abuse and bounce processing to stop.
			_IsStopping = true;
			while (_BounceProcessingRunning || _AbuseProcessingRunning)
				System.Threading.Thread.Sleep(50);
		}

		public void Start()
		{
			// Start the events forwarder.
			_events.Start();
		}

		/// <summary>
		/// Filesystem watcher callback for Bounced Emails directory.
		/// </summary>
		/// <param name="sender">FileSystemWatcher</param>
		/// <param name="e"></param>
		private void DoBounceFileProcessing(object sender, FileSystemEventArgs e)
		{
			FileSystemWatcher bounceWatcher = (FileSystemWatcher)sender;

			lock (_Lock)
			{
				// If bounce processing is already running don't need to do anything.
				if (_BounceProcessingRunning)
					return;

				// Bounce processing wasn't running so set flag to true and disable FSW.
				_BounceProcessingRunning = true;
				bounceWatcher.EnableRaisingEvents = false;
			}

			try
			{
				// Run the bounce processing task.
				DirectoryHandler(_config.BounceDropFolder, _eventsManager.ProcessBounceEmail);
			}
			catch (Exception ex)
			{
				// Warn if anything went wrong.
				_logging.Debug("DoBounceFileProcessing something went wrong.", ex);
			}
			finally
			{
				// Always clear the isprocessing flag and restart the filesystem watcher.
				_BounceProcessingRunning = false;
				bounceWatcher.EnableRaisingEvents = true;
			}
		}

		/// <summary>
		/// File system watcher callback for the Abuse.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void DoAbuseFileProcessing(object sender, FileSystemEventArgs e)
		{
			FileSystemWatcher abuseWatcher = (FileSystemWatcher)sender;
			lock (_Lock)
			{
				if (_AbuseProcessingRunning)
					return;

				_AbuseProcessingRunning = true;
				abuseWatcher.EnableRaisingEvents = false;
			}

			try
			{
				Task.Run(() =>
				{
					DirectoryHandler(_config.FeedbackLoopDropFolder, _eventsManager.ProcessFeedbackLoop);
				});
			}
			catch (Exception ex)
			{
				_logging.Warn("DoAbuseFileProcessing something went wrong.", ex);
			}
			finally
			{
				_AbuseProcessingRunning = false;
				abuseWatcher.EnableRaisingEvents = true;
			}
		}

		/// <summary>
		/// DirectoryHandler provides a standard way of processing a directory of email files.
		/// </summary>
		/// <param name="path">The filepath to operate on.</param>
		/// <param name="fileProcessor">A delegate method that will be used to process each file found in <paramref name="path"/>.</param>
		/// <param name="logger">A delegate method that will be used to return information to an interface, e.g. to
		/// display messages to a user.</param>
		private void DirectoryHandler(string path, Func<string, EmailProcessingDetails> fileProcessor)
		{
			// A filter to use when pulling out files to process; likely to be "*.eml".
			string fileSearchPattern = "*.eml";

			FileInfo[] files = new DirectoryInfo(path).GetFiles(fileSearchPattern);

			// Keep going until there aren't any more files to process, then we wait for the FileSystemWatcher to nudge us
			// back into life again.
			do
			{
				// Loop through and process all the files we've picked up.
				Parallel.ForEach<FileInfo>(files, delegate (FileInfo f)
				{
					if (_IsStopping)
						return;

					if (!File.Exists(f.FullName))
					{
						_logging.Debug(String.Format("File not found: \"{0}\".", f.FullName));
						return;
					}

					// If a file's not accessible, skip it so we'll pick it up the next time.
					if (IsFileLocked(f))
						return;

					string content = File.ReadAllText(f.FullName);

					// Send the content to the delegate method that'll process its contents.
					EmailProcessingDetails processingDetails = fileProcessor(content);

					switch (processingDetails.ProcessingResult)
					{
						case EmailProcessingResult.SuccessAbuse:
							// All good.  Nothing to do other than delete the file.
							File.Delete(f.FullName);
							break;

						case EmailProcessingResult.SuccessBounce:

							// To enable reviewing of bounce emails and how Manta has processed them, a flag can be set that
							// keeps processed files.  Files that result in an error when being processed are always kept.
							if (_config.KeepBounceFiles)
							{
								// Retain files.

								string keepPath = Path.Combine(Path.GetDirectoryName(f.FullName), _SubdirectoryForProcessedBounceEmails, processingDetails.BounceIdentifier.ToString());

								switch (processingDetails.BounceIdentifier)
								{
									case BounceIdentifier.BounceRule:
										keepPath = Path.Combine(keepPath, processingDetails.MatchingBounceRuleID.ToString());
										break;

									case BounceIdentifier.NdrCode:
									case BounceIdentifier.SmtpCode:
										keepPath = Path.Combine(keepPath, processingDetails.MatchingValue);
										break;
								}

								Directory.CreateDirectory(keepPath);

								File.Move(f.FullName, Path.Combine(keepPath, f.Name));
							}
							else
							{
								// All good.  Nothing to do other than delete the file.
								File.Delete(f.FullName);
							}

							break;

						case EmailProcessingResult.ErrorNoFile:
							throw new FileNotFoundException("Failed to locate file to process: \"" + f.Name + "\".");

						case EmailProcessingResult.Unknown:
						case EmailProcessingResult.ErrorContent:
						case EmailProcessingResult.ErrorNoReason:
						default:
							// Move the file into a separate directory, handling any issues of duplicate filenames.
							bool moved = false;
							int version = 0;
							Stopwatch sw = new Stopwatch();
							sw.Start();
							while (!moved)
							{
								try
								{
									if (sw.Elapsed > TimeSpan.FromSeconds(10))
										throw new TimeoutException();

									if (version > 0)
									{
										File.Move(f.FullName, Path.Combine(Path.GetDirectoryName(f.FullName), _SubdirectoryForProblemEmails, Path.GetFileNameWithoutExtension(f.Name) + "_" + version.ToString() + f.Extension));
									}
									else
										File.Move(f.FullName, Path.Combine(Path.GetDirectoryName(f.FullName), _SubdirectoryForProblemEmails, f.Name));

									moved = true;
								}
								catch (TimeoutException)
								{
									_logging.Fatal("Tried to move file " + f.Name + " for 10 seconds but failed.");
									_logging.Fatal("This is a FATAL failure.");
									Environment.Exit(-1);
								}
								catch (Exception)
								{
									_logging.Debug("Attempt " + version.ToString() + " Failed.");
									version++;
								}
							}
							break;
					}
				});

				if (!_IsStopping)
				{
					// Get any new files that have turned up.
					files = new DirectoryInfo(path).GetFiles(fileSearchPattern);
				}
			}
			while (files.Count() > 0);
		}

		/// <summary>
		/// Checks whether a file can be opened.
		/// </summary>
		/// <param name="file">The file to check.</param>
		/// <returns>true if the file is locked, else false.</returns>
		private bool IsFileLocked(FileInfo file)
		{
			FileStream stream = null;
			try
			{
				stream = file.Open(FileMode.Open, FileAccess.ReadWrite, FileShare.None);
			}
			catch (IOException)
			{
				return true;
			}
			finally
			{
				if (stream != null)
					stream.Close();
			}
			return false;
		}
	}
}