using System;
using System.Text;
using BinaryRage.Functions;

namespace BinaryRage
{
	/// <summary>
	/// Represents a KeyValue store
	/// </summary>
    public class BinaryCache
    {
		private readonly IStorage storage;

        public BinaryCache(string storeName, IStorage? storage = null)
		{
			this.storeName = storeName;
			this.storage = storage == null ? new Storage() : storage;
		}

		static readonly char[] invalid = Path.GetInvalidFileNameChars();
		private readonly string storeName;

		public string StoreName => this.storeName;

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

        private string ComputeKey(object rawKey)
        {
            if (rawKey == null)
                throw new ArgumentNullException( nameof( rawKey ) );

			if (rawKey is string)
				return NormalizeKey( (string) rawKey );

            return NormalizeKey( Convert.ToBase64String( ConvertHelper.ObjectToByteArray( rawKey ) ) );
		}

		private string CacheKey( string key ) => this.storeName + key;

		public async Task Set<T>(object rawKey, T value)
        {
            var key = ComputeKey( rawKey );
            SimpleObject simpleObject = new SimpleObject ( key, value, this.storeName );

            Cache.CacheDic[CacheKey(key)] = simpleObject;

            await this.storage.Write(simpleObject.Key, await Compress.CompressGZip(ConvertHelper.ObjectToByteArray(value!)),
                simpleObject.Store);
        }

        public void Remove(object rawKey)
        {
			var key = ComputeKey(rawKey);
			Cache.CacheDic.Remove( CacheKey( key ), out _ );
            this.storage.Remove( key, this.storeName );
        }

		public async Task<T?> Get<T>(object rawKey)
        {
			var key = ComputeKey(rawKey);

			SimpleObject? simpleObjectFromCache;
			if (Cache.CacheDic.TryGetValue( CacheKey( key ), out simpleObjectFromCache ))
                return (T?)simpleObjectFromCache.Value;

            byte[] compressGZipData = await Compress.DecompressGZip(await this.storage.Read(key, this.storeName));
            var umcompressedObject = (T?) ConvertHelper.ByteArrayToObject(compressGZipData);
            Cache.CacheDic.TryAdd( CacheKey( key ), new SimpleObject (key, umcompressedObject, this.storeName ) );
            return umcompressedObject;
        }

        public bool Exists( object rawKey )
        {
			var key = ComputeKey(rawKey);

			if (Cache.CacheDic.ContainsKey( CacheKey( key ) )) 
				return true;

			return this.storage.Exists( key, this.storeName );
        }
    }
}
