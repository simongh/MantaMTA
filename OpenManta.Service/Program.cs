using Topshelf;
using Topshelf.Ninject;

namespace OpenManta.Service
{
	internal class Program
	{
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		private static void Main()
		{
			HostFactory.Run(host =>
			{
				host.UseLog4Net();

				host.UseNinject(new ServiceModule(), new Framework.FrameworkModule(), new Core.CoreModule(), new Data.DataModule());

				host.Service<OpenMantaService>(sc =>
				{
					sc.ConstructUsingNinject();

					sc.WhenStarted(s => s.Start());
					sc.WhenStopped(s => s.Stop());
				});

				host.RunAsLocalSystem();

				host.SetDescription("The OpenManta mail transfer agent");
				host.SetDisplayName("OpenManta");
				host.SetServiceName("OpenManta");

				host.DependsOnMsSql();
			});
		}
	}

	public class testservice
	{
		public void Start()
		{ }

		public void Stop()
		{ }
	}
}