using Ninject;
using OpenManta.Core;

namespace OpenManta.Framework
{
	public static class MtaParametersFactory
	{
		private static IKernel _kernel;

		internal static void Initialise(IKernel kernel)
		{
			Guard.NotNull(kernel, nameof(kernel));

			_kernel = kernel;
		}

		public static IMtaParameters Create()
		{
			return _kernel.Get<IMtaParameters>();
		}
	}
}