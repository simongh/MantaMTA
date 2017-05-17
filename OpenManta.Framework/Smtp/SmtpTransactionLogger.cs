﻿using System;
using System.IO;
using System.Threading;
using log4net;
using OpenManta.Core;

namespace OpenManta.Framework.Smtp
{
	internal class SmtpTransactionLogger : ISmtpTransactionLogger
	{
		/// <summary>
		/// Identifies the current log hour
		/// </summary>
		private int _CurrentLogHour = -1;

		/// <summary>
		/// Stream writer is used for writing to the log file
		/// </summary>
		private StreamWriter _Writer = null;

		/// <summary>
		/// Log file writer lock.
		/// </summary>
		private object writeLock = new object();

		private bool _disposed;
		private readonly ILog _logging;
		private readonly IMtaParameters _config;

		private SmtpTransactionLogger(ILog logging, IMtaParameters config)
		{
			Guard.NotNull(logging, nameof(logging));
			Guard.NotNull(config, nameof(config));

			_logging = logging;
			_config = config;

			// Handle any uncaught exceptions, need to flush and close the logging streams
			AppDomain.CurrentDomain.UnhandledException += delegate (object sender, UnhandledExceptionEventArgs e)
			{
				// If writer is open and can be written
				// then we should flush and close
				if (_Writer != null &&
					 _Writer.BaseStream != null &&
					 _Writer.BaseStream.CanWrite)
				{
					_Writer.Flush();
					_Writer.Close();
				}
			};

			// The interval in milliseconds between delete old logs callback.
			int intervalBetweenDeleteCallbacksMilliseconds = 60 * 60 * 1000;
			_DeleteOldLogsTimer = new Timer(new TimerCallback(delegate (object state)
			{
				try
				{
					// Get the files in the log folder
					FileInfo[] files = new DirectoryInfo(_config.MTA_SMTP_LOGFOLDER).GetFiles();

					// This is the date that will be used to delete files before.
					DateTimeOffset deleteCreatedBefore = DateTimeOffset.UtcNow.AddDays(_config.DaysToKeepSmtpLogsFor * -1);

					// Loop through all the log folder files.
					for (int i = 0; i < files.Length; i++)
					{
						if (files[i].CreationTimeUtc < deleteCreatedBefore)
						{
							try
							{
								// The log file is older than we want to keep so attempt to delete it.
								files[i].Delete();
							}
							catch (Exception ex)
							{
								// Something went wrong trying to delete the log file.
								_logging.Error("Failed to delete old log file (" + files[i].Name + ")", ex);
							}
						}
					}
				}
				catch (Exception ex)
				{
					// Something, not delete related, went wrong.
					_logging.Error("Deleting old log files", ex);
				}
				finally
				{
					// Were done deleting so tell the timer to do the callback again in n.
					_DeleteOldLogsTimer.Change(intervalBetweenDeleteCallbacksMilliseconds, Timeout.Infinite);
				}
			}), null, 1 * 1000, Timeout.Infinite);
		}

		~SmtpTransactionLogger()
		{
			Dispose(false);
		}

		/// <summary>
		/// Time for the delete old logs timer.
		/// </summary>
		private Timer _DeleteOldLogsTimer { get; set; }

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected void Dispose(bool disposing)
		{
			if (_disposed)
				return;

			if (_Writer != null)
				_Writer.Dispose();

			_disposed = true;
		}

		/// <summary>
		/// Write a message to the log file
		/// </summary>
		/// <param name="msg"></param>
		public void Log(string msg)
		{
			lock (writeLock)
			{
				// Ensure logging by hour
				if (DateTimeOffset.UtcNow.Hour != _CurrentLogHour && _Writer != null)
				{
					_Writer.Flush();
					_Writer.Close();
					_Writer = null;
				}

				// If the stream writer doesn't exist, the filestream doesn't exist or is not writeable
				if (_Writer == null || _Writer.BaseStream == null || !_Writer.BaseStream.CanWrite)
				{
					try
					{
						_Writer = new StreamWriter(GetCurrentLogPath(), true);
					}
					catch (IOException)
					{
						_Writer = new StreamWriter(GetCurrentLogPath() + "2", true);
					}
				}

				_Writer.WriteLine(GetCurrentTimestamp() + " " + msg.TrimEnd());
				_Writer.Flush();
				_Writer.BaseStream.Flush();
			}
		}

		/// <summary>
		/// Works out the current log file path, log files use date time to the hour
		/// </summary>
		/// <returns></returns>
		private string GetCurrentLogPath()
		{
			_CurrentLogHour = DateTimeOffset.UtcNow.Hour;
			return Path.Combine(_config.MTA_SMTP_LOGFOLDER, DateTimeOffset.UtcNow.ToString("yyyyMMddHH") + ".txt");
		}

		/// <summary>
		/// Return a string containing the current date/time in the format used
		/// for logging
		/// </summary>
		/// <returns></returns>
		private string GetCurrentTimestamp()
		{
			return DateTimeOffset.UtcNow.ToString("yyyy/MM/dd HH:mm:ss.ff");
		}
	}
}