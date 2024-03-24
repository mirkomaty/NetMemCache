using System;
using System.Text;
using BinaryRage.Interfaces;

namespace BinaryRage
{
	/// <summary>
	/// Represents a KeyValue store
	/// </summary>
    public class BinaryCache
    {
		private readonly IStorage storage;

        public BinaryCache(string storeName, IStorage? storage = null, IFolderStructure? folderStructure = null, IObjectSerializer? objectSerializer = null)
		{
			this.storeName = storeName;
			this.objectSerializer = objectSerializer != null ? objectSerializer : new ObjectSerializer();
			this.folderStructure = folderStructure != null ? folderStructure : new FolderStructure();
			this.storage = storage != null ? storage : new Storage(this.folderStructure);
		}

		static readonly char[] invalid = Path.GetInvalidFileNameChars();
		private readonly string storeName;
		private readonly IObjectSerializer objectSerializer;
		private readonly IFolderStructure folderStructure;

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

            return NormalizeKey( this.objectSerializer.SerializeKey( rawKey ) );
		}

		private string CacheKey( string key ) => this.storeName + key;

		public async Task Set<T>(object rawKey, T value)
        {
            var key = ComputeKey( rawKey );
            SimpleObject simpleObject = new SimpleObject ( key, value, this.storeName );

            Cache.CacheDic[CacheKey(key)] = simpleObject;

            await this.storage.Write(simpleObject.Key, await this.objectSerializer.Serialize(value),
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
			var rawData = await this.storage.Read(key, this.storeName);
			T? uncompressedObject = (T?) await this.objectSerializer.Deserialize( rawData );
			Cache.CacheDic.TryAdd( CacheKey( key ), new SimpleObject (key, uncompressedObject, this.storeName ) );
			return uncompressedObject;
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
