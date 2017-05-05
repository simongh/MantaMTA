using Ninject;

namespace OpenManta.Core
{
	public class CoreModule : Ninject.Modules.NinjectModule
	{
		public override void Load()
		{
			Bind<IMimeMessageParser>().To<MimeMessageParser>();

			MimeMessageParserFactory.Instance = Kernel.Get<IMimeMessageParser>();
		}
	}
}