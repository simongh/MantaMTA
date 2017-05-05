using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using OpenManta.Core;
using OpenManta.Data;
using Newtonsoft.Json;
using log4net;

namespace OpenManta.Framework
{
	public class EventHttpForwarder : IEventHttpForwarder
	{
		private volatile bool _IsStopping;

		// Should be set to true when processing events and false when done.
		private bool _IsRunning;

		private readonly IEventDB _eventDb;
		private readonly ILog _logging;
		private readonly IMantaCoreEvents _coreEvents;
		private readonly IEventsManager _events;
		private readonly IMtaParameters _config;

		public EventHttpForwarder(IEventDB eventDb, ILog logging, IMantaCoreEvents coreEvents, IEventsManager events, IMtaParameters config)
		{
			Guard.NotNull(eventDb, nameof(eventDb));
			Guard.NotNull(logging, nameof(logging));
			Guard.NotNull(coreEvents, nameof(coreEvents));
			Guard.NotNull(events, nameof(events));
			Guard.NotNull(config, nameof(config));

			_eventDb = eventDb;
			_logging = logging;
			_coreEvents = coreEvents;
			_events = events;
			_config = config;

			_IsStopping = false;
			_IsRunning = false;
		}

		/// <summary>
		/// IStopRequired method. Will stop the EventHttpForwarder when the MTA is stopping.
		/// </summary>
		public void Stop()
		{
			_IsStopping = true;
			// Wait until EventHttpForwarder has stopped.
			while (_IsRunning)
				Thread.Sleep(50);
		}

		/// <summary>
		/// Call this method to start the EventHttpForwarder.
		/// </summary>
		public void Start()
		{
			if (_config.EventForwardingHttpPostUrl != null)
			{
				var t = new Thread(new ThreadStart(ForwardEvents));
				t.Start();
			}
		}

		/// <summary>
		/// Does the actual forwarding of the events.
		/// </summary>
		private void ForwardEvents()
		{
			_IsRunning = true;

			try
			{
				// Keep looping as long as the MTA is running.
				while (!_IsStopping)
				{
					IList<MantaEvent> events = null;
					// Get events for forwarding.
					try
					{
						events = _eventDb.GetEventsForForwarding(10);
					}
					catch (SqlNullValueException)
					{
						events = new List<MantaEvent>();
					}

					if (events.Count == 0)
					{
						// No events to forward sleep for a second and look again.
						Thread.Sleep(1000);
						continue;
					}
					else
					{
						// Found events to forward, create and run Tasks to forward.
						//for (var i = 0; i < events.Count; i++)
						Parallel.ForEach(events, e =>
						{
							ForwardEventAsync(e).GetAwaiter().GetResult();
						});
					}
				}
			}
			catch (Exception ex)
			{
				// Something went wrong.
				_logging.Error("EventHttpForwarder encountered an error.", ex);
				_coreEvents.InvokeMantaCoreStopping();
				Environment.Exit(-1);
			}

			_IsRunning = false;
		}

		private async Task ForwardEventAsync(MantaEvent evt)
		{
			try
			{
				if (_IsStopping)
					return;

				// Create the HTTP POST request to the remove endpoint.
				var httpRequest = (HttpWebRequest)WebRequest.Create(_config.EventForwardingHttpPostUrl);
				httpRequest.Method = "POST";
				httpRequest.ContentType = "text/json";

				// Convert the Event to JSON.
				string eventJson = string.Empty;
				switch (evt.EventType)
				{
					case MantaEventType.Abuse:
						eventJson = JsonConvert.SerializeObject((MantaAbuseEvent)evt);
						break;

					case MantaEventType.Bounce:
						eventJson = JsonConvert.SerializeObject((MantaBounceEvent)evt);
						break;

					case MantaEventType.TimedOutInQueue:
						eventJson = JsonConvert.SerializeObject((MantaTimedOutInQueueEvent)evt);
						break;

					default:
						eventJson = JsonConvert.SerializeObject(evt);
						break;
				}

				// Remove the forwarded property as it is internal only.
				eventJson = Regex.Replace(eventJson, ",\"Forwarded\":(false|true)", string.Empty);

				// Write the event json to the POST body.
				using (StreamWriter writer = new StreamWriter(await httpRequest.GetRequestStreamAsync()))
				{
					await writer.WriteAsync(eventJson);
				}

				// Send the POST and get the response.
				HttpWebResponse httpResponse = (HttpWebResponse)await httpRequest.GetResponseAsync();

				// Get the response body.
				string responseBody = string.Empty;
				using (StreamReader reader = new StreamReader(httpResponse.GetResponseStream()))
				{
					responseBody = await reader.ReadToEndAsync();
				}

				// If response body is just a "." then event was received successfully.
				if (responseBody.Trim().StartsWith("."))
				{
					// Log that the event forwared.
					evt.Forwarded = true;
					await _events.SaveAsync(evt);
				}
			}
			catch (Exception ex)
			{
				// We failed to forward the event. Most likly because the remote server didn't respond.
				_logging.Error("Failed to forward event " + evt.ID, ex);
			}
		}
	}
}