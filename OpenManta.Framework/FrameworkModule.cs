using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenManta.Framework
{
	public class FrameworkModule : Ninject.Modules.NinjectModule
	{
		public override void Load()
		{
			Bind<IBounceRulesManager>().To<BounceRulesManager>().InSingletonScope();
			Bind<IDnsApi>().To<dnsapi>();
			Bind<IDnsManager>().To<DNSManager>();
			Bind<IEventHttpForwarder>().To<EventHttpForwarder>().InSingletonScope();
			Bind<IEventsFileHandler>().To<EventsFileHandler>().InSingletonScope();
			Bind<IEventsManager>().To<EventsManager>().InSingletonScope();
			Bind<IMantaCoreEvents>().To<MantaCoreEvents>();
			Bind<IMessageManager>().To<MessageManager>();
			Bind<IMessageSender>().To<MessageSender>().InSingletonScope();
			Bind<IOutboundRuleManager>().To<OutboundRuleManager>();
			Bind<IQueueManager>().To<QueueManager>().InSingletonScope();
			Bind<ISmtpServerTransaction>().To<SmtpServerTransaction>();
			Bind<IThrottleManager>().To<ThrottleManager>().InSingletonScope();
			Bind<IVirtualMtaManager>().To<VirtualMtaManager>();
			Bind<IMtaParameters>().To<MtaParameters>();

			Bind<Smtp.IMantaSmtpClientPoolCollection>().To<Smtp.MantaSmtpClientPoolCollection>().InSingletonScope();
			Bind<Smtp.IMantaOutboundClientPool>().To<Smtp.MantaOutboundClientPool>();
			Bind<Smtp.IMantaOutboundClient>().To<Smtp.MantaOutboundClient>();
			Bind<Smtp.ISmtpStreamHandler>().To<Smtp.SmtpStreamHandler>();
			Bind<Smtp.ISmtpTransactionLogger>().To<Smtp.SmtpTransactionLogger>().InSingletonScope();

			Bind<Smtp.IOutboundClientFactory>().To<Smtp.SmtpFactory>();
			Bind<Smtp.ISmtpServerFactory>().To<Smtp.SmtpFactory>();
		}
	}
}