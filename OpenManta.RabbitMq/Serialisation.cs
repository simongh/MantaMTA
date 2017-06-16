using Newtonsoft.Json;
using System;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Threading.Tasks;

namespace OpenManta.Framework
{
    /// <summary>
    /// Provides methods to (de)serialise .Net objects to JSON and back.
    /// </summary>
    internal static class Serialisation
	{
        /// <summary>
        /// Deserialises the JSON or gzipped JSON to an object of type.
        /// </summary>
        /// <typeparam name="T">Type of object to deserialise to.</typeparam>
        /// <param name="obj">JSON or gziped JSON bytes.</param>
        /// <returns>An object of T, with deserialised values.</returns>
        public static async Task<T> Deserialise<T>(byte[] obj)
        {
            // Will hold the JSON to deserialise.
            string json = string.Empty;

            try
            {
                // Attempt to decompress the bytes, if exception is thrown then not compressed.
                using (var msCompressed = new MemoryStream(obj))
                {
                    using (var msUncompressed = new MemoryStream())
                    {
                        using (var gs = new GZipStream(msCompressed, CompressionMode.Decompress))
                        {
                            await gs.CopyToAsync(msUncompressed);
                        }

                        json = Encoding.UTF8.GetString(msUncompressed.ToArray());
                    }
                }
            }
            catch (Exception)
            {
                // Not compressed so just get the string.
                json = Encoding.UTF8.GetString(obj);
            }

            // Return the deserialised object.
            return JsonConvert.DeserializeObject<T>(json);
        }

        /// <summary>
        /// Serialise the object to JSON. Will gzip the JSON by default.
        /// </summary>
        /// <param name="obj">Object to serialise.</param>
        /// <param name="compress">If true will return compress the JSON with gzip.</param>
        /// <returns>Byte array of JSON or gizped JSON.</returns>
        public static async Task<byte[]> Serialise(object obj, bool compress = true)
		{
            // Serialise the object.
            string json = JsonConvert.SerializeObject(obj);
			byte[] bytes = Encoding.UTF8.GetBytes(json);

			// Not compressing so just return the JSON.
			if (!compress)
				return bytes;

			// Do the Compression.
			using (var msUncompressed = new MemoryStream(bytes))
			{
				using (var msCompressed = new MemoryStream())
				{
					using (var gzip = new GZipStream(msCompressed, CompressionMode.Compress))
					{
						await msUncompressed.CopyToAsync(gzip);
					}

					return msCompressed.ToArray();
				}
			}
		}
	}
}
