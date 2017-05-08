namespace OpenManta.Service
{
	public class ServiceModule : Ninject.Modules.NinjectModule
	{
		public override void Load()
		{
			Bind<ISmtpServerFactory>().To<SmtpServerFactory>();
		}
	}
}