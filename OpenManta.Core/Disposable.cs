using System;

namespace OpenManta.Core
{
	public abstract class Disposable : IDisposable
	{
		protected bool IsDisposed;

		public void Dispose()
		{
			Dispose(true);

			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			IsDisposed = true;
		}

		~Disposable()
		{
			Dispose(false);
		}
	}
}