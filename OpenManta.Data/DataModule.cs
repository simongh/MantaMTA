using Ninject;

namespace OpenManta.Data
{
	public class DataModule : Ninject.Modules.NinjectModule
	{
		public override void Load()
		{
			Bind<IMantaDB>().To<MantaDB>();

			Bind<ICfgLocalDomains>().To<CfgLocalDomains>();
			Bind<ICfgPara>().To<CfgPara>();
			Bind<ICfgRelayingPermittedIP>().To<CfgRelayingPermittedIP>();
			Bind<IDataRetrieval>().To<DataRetrieval>();
			Bind<IEventDB>().To<EventDB>();
			Bind<IFeedbackLoopEmailAddressDB>().To<FeedbackLoopEmailAddressDB>();
			Bind<IMtaMessageDB>().To<MtaMessageDB>();
			Bind<IMtaTransaction>().To<MtaTransaction>();
			Bind<IOutboundRuleDB>().To<OutboundRuleDB>();
			Bind<ISendDB>().To<SendDB>();
			Bind<IVirtualMtaDB>().To<VirtualMtaDB>();
			Bind<IVirtualMtaGroupDB>().To<VirtualMtaGroupDB>();

			MantaDbFactory.Instance = Kernel.Get<IMantaDB>();
			CfgLocalDomainsFactory.Instance = Kernel.Get<ICfgLocalDomains>();
			CfgParaFactory.Instance = Kernel.Get<ICfgPara>();
			CfgRelayingPermittedIPFactory.Instance = Kernel.Get<ICfgRelayingPermittedIP>();
			EventDbFactory.Instance = Kernel.Get<IEventDB>();
			FeedbackLoopEmailAddressDBFactory.Instance = Kernel.Get<IFeedbackLoopEmailAddressDB>();
			MtaMessageDBFactory.Instance = Kernel.Get<IMtaMessageDB>();
			MtaTransactionFactory.Instance = Kernel.Get<IMtaTransaction>();
			OutboundRuleDBFactory.Instance = Kernel.Get<IOutboundRuleDB>();
			SendDBFactory.Instance = Kernel.Get<ISendDB>();
			VirtualMtaDBFactory.Instance = Kernel.Get<IVirtualMtaDB>();
			VirtualMtaGroupDBFactory.Instance = Kernel.Get<IVirtualMtaGroupDB>();
		}
	}
}