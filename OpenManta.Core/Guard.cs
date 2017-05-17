using System;
using System.Diagnostics;

namespace OpenManta.Core
{
	[DebuggerStepThrough]
	public static class Guard
	{
		public static void NotNull(object value, string paramName)
		{
			if (value == null)
				throw new ArgumentNullException(paramName);
		}

		public static void NotNullOrEmpty(string value, string paramName)
		{
			if (string.IsNullOrEmpty(value))
				throw new ArgumentNullException(paramName);
		}
	}
}