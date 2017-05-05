using System.Net.Sockets;
using Ninject;
using Ninject.Parameters;
using OpenManta.Core;

namespace OpenManta.Framework.Smtp
{
	internal class SmtpFactory : IOutboundClientFactory, ISmtpServerFactory
	{
		private readonly IKernel _kernel;

		public SmtpFactory(IKernel kernel)
		{
			Guard.NotNull(kernel, nameof(kernel));

			_kernel = kernel;
		}

		public IMantaOutboundClient GetOutboundClient(VirtualMTA vmta, MXRecord mxRecord)
		{
			return _kernel.Get<IMantaOutboundClient>(new ConstructorArgument("vmta", vmta), new ConstructorArgument("mxRecord", mxRecord));
		}

		public IMantaOutboundClientPool GetOutboundClientPool(VirtualMTA vmta, MXRecord mxRecord)
		{
			return _kernel.Get<IMantaOutboundClientPool>(new ConstructorArgument("vmta", vmta), new ConstructorArgument("mxRecord", mxRecord));
		}

		public ISmtpStreamHandler GetHandler(TcpClient client)
		{
			Guard.NotNull(client, nameof(client));

			var result = _kernel.Get<ISmtpStreamHandler>();
			result.Open(client);

			return result;
		}

		public ISmtpServerTransaction GetTransaction()
		{
			return _kernel.Get<ISmtpServerTransaction>();
		}
	}
}