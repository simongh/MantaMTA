using System;

namespace OpenManta.Framework
{
	/// <summary>
	/// Interface is for objects that need to be stopped when the MTA is stopping.
	/// </summary>
	internal interface IStopRequired
	{
		void Stop();
	}
}
