using OpenManta.Framework.RabbitMq;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OpenManta.Framework
{
    public static class MantaCoreEvents
	{
		/// <summary>
		/// List of all the objects that need to be stopped.
		/// </summary>
		private static List<IStopRequired> _StopRequiredTasks = new List<IStopRequired>();

		/// <summary>
		/// Registers an instance of a class that implements IStopRequired.
		/// </summary>
		/// <param name="instance">Thing that needs to be stopped.</param>
		internal static void RegisterStopRequiredInstance(IStopRequired instance)
		{
			_StopRequiredTasks.Add(instance);
		}

		/// <summary>
		/// This should be called when the MTA is stopping as it will stop stuff that needs stopping.
		/// </summary>
		public static void InvokeMantaCoreStopping()
		{
            Logging.Debug("InvokeMantaCoreStopping Started.");

            // Loop through the things that need stopping and stop them :)
            Parallel.ForEach(_StopRequiredTasks, instance =>
            {
                Logging.Debug("InvokeMantaCoreStopping > " + instance.GetType());
                instance.Stop();
            });

            // Close the RabbitMQ connection when were done.
            RabbitMqManager.LocalhostConnection.Close();

            Logging.Debug("InvokeMantaCoreStopping Finished.");
		}
	}
}
