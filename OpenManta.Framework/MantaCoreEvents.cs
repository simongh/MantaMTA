using System.Collections.Generic;
using System.Threading.Tasks;
using log4net;
using OpenManta.Core;

namespace OpenManta.Framework
{
	internal class MantaCoreEvents : IMantaCoreEvents
	{
		/// <summary>
		/// List of all the objects that need to be stopped.
		/// </summary>
		private List<IStopRequired> _StopRequiredTasks;

		private readonly ILog _logging;

		//private readonly IRabbitMqManager _manager;
		private readonly Queues.IManager _queue;

		public MantaCoreEvents(ILog logging, Queues.IManager queue)
		{
			Guard.NotNull(logging, nameof(logging));
			Guard.NotNull(queue, nameof(queue));

			_logging = logging;
			//_manager = manager;
			_queue = queue;
			_StopRequiredTasks = new List<IStopRequired>();
		}

		/// <summary>
		/// Registers an instance of a class that implements IStopRequired.
		/// </summary>
		/// <param name="instance">Thing that needs to be stopped.</param>
		public void RegisterStopRequiredInstance(IStopRequired instance)
		{
			_StopRequiredTasks.Add(instance);
		}

		/// <summary>
		/// This should be called when the MTA is stopping as it will stop stuff that needs stopping.
		/// </summary>
		public void InvokeMantaCoreStopping()
		{
			_logging.Debug("InvokeMantaCoreStopping Started.");

			// Loop through the things that need stopping and stop them :)
			Parallel.ForEach(_StopRequiredTasks, instance =>
			{
				_logging.Debug("InvokeMantaCoreStopping > " + instance.GetType());
				instance.Stop();
			});

			// Close the RabbitMQ connection when were done.
			//_manager.Close();
			_queue.Close();

			_logging.Debug("InvokeMantaCoreStopping Finished.");
		}
	}
}