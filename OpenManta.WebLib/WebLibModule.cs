namespace OpenManta.WebLib
{
	public class WebLibModule : Ninject.Modules.NinjectModule
	{
		public override void Load()
		{
			Bind<IOutboundRuleWebManager>().To<OutboundRuleWebManager>();
			Bind<IVirtualMtaWebManager>().To<VirtualMtaWebManager>();
			Bind<DAL.IOutboundRulesDB>().To<DAL.OutboundRulesDB>();

			Bind<DAL.ISendDB>().To<DAL.SendDB>();
			Bind<DAL.ITransactionDB>().To<DAL.TransactionDB>();
			Bind<DAL.IVirtualMtaDB>().To<DAL.VirtualMtaDB>();
			Bind<DAL.IVirtualMtaTransactionDB>().To<DAL.VirtualMtaTransactionDB>();
		}
	}
}