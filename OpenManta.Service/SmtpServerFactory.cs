using System.Net;
using Ninject;
using OpenManta.Core;

namespace OpenManta.Service
{
	internal class SmtpServerFactory : ISmtpServerFactory
	{
		private readonly IKernel _kernel;

		public SmtpServerFactory(IKernel kernel)
		{
			Guard.NotNull(kernel, nameof(kernel));

			_kernel = kernel;
		}

		public Framework.ISmtpServer Create()
		{
			return _kernel.Get<Framework.ISmtpServer>();
		}
	}
}