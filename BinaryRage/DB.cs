using System;
using System.Text;
using BinaryRage.Functions;

namespace BinaryRage
{
    static public class DB
    {
		static char[] invalid = Path.GetInvalidFileNameChars();

		private static string NormalizeKey(string key)
        {
			StringBuilder sb = new StringBuilder();
			foreach (var c in key)
			{
				if (invalid.Contains( c ))
				{
					var bytes = Encoding.UTF8.GetBytes( new[] { c } );
					foreach (byte b in bytes)
					{
						sb.Append( b.ToString( "X2" ) );
					}
				}
				else
				{
					sb.Append( c );
				}
			}

            return sb.ToString();
		}

        static async public Task Insert<T>(string rawKey, T value, string filelocation)
        {
            var key = NormalizeKey(rawKey);
            SimpleObject simpleObject = new SimpleObject { Key = key, Value = value, FileLocation = filelocation };

            Cache.CacheDic[filelocation + key] = simpleObject;

            await Storage.WriteToStorage(simpleObject.Key, await Compress.CompressGZip(ConvertHelper.ObjectToByteArray(value)),
                simpleObject.FileLocation);
        }

        static public void Remove(string rawKey, string fileLocation)
        {
			var key = NormalizeKey(rawKey);
			Cache.CacheDic.Remove(fileLocation + key, out _);
            Storage.Remove( key, fileLocation );
        }

        static async public Task<T> Get<T>(string rawKey, string filelocation)
        {
			var key = NormalizeKey(rawKey);

			SimpleObject simpleObjectFromCache;
            if (Cache.CacheDic.TryGetValue(filelocation + key, out simpleObjectFromCache))
                return (T)simpleObjectFromCache.Value;

            byte[] compressGZipData = await Compress.DecompressGZip(await Storage.GetFromStorage(key, filelocation));
            T umcompressedObject = (T) ConvertHelper.ByteArrayToObject(compressGZipData);
            Cache.CacheDic.TryAdd( key, new SimpleObject { Key = key, Value = umcompressedObject, FileLocation = filelocation } );
            return umcompressedObject;
        }

        static public string GetJSON<T>(string key, string filelocation)
        {
            return SimpleSerializer.Serialize(Get<T>(key, filelocation));
        }

        static public bool Exists(string rawKey, string filelocation)
        {
			var key = NormalizeKey(rawKey);
			return Storage.ExistingStorageCheck(key, filelocation);
        }
    }
}
