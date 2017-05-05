using System;

namespace OpenManta.Framework.Smtp
{
	internal interface ISmtpTransactionLogger : IDisposable
	{
		void Log(string msg);
	}
}