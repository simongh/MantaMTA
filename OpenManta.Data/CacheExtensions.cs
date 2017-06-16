using System;
using System.Runtime.Caching;

namespace OpenManta.Data
{
	public static class CacheExtensions
	{
		public static T GetValue<T>(this ObjectCache cache, string key, Func<T> loader)
		{
			if (cache.Contains(key))
				return (T)cache[key];

			return (T)cache.AddOrGetExisting(key, loader(), DateTimeOffset.UtcNow.AddMinutes(15));
		}
	}
}