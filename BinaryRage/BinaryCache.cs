using System;
using System.Collections.Concurrent;
using System.Text;
using BinaryRage.Interfaces;

namespace BinaryRage
{
	/// <summary>
	/// Represents a KeyValue store
	/// </summary>
    public class BinaryCache
    {
		private static ConcurrentDictionary<string, CacheEntry> cacheDictionary = new ConcurrentDictionary<string, CacheEntry>();
		private readonly IStorage storage;

		public BinaryCache(string storeName, IStorage? storage = null, IFolderStructure? folderStructure = null, IObjectSerializer? objectSerializer = null)
		{
			this.storeName = storeName;
			this.objectSerializer = objectSerializer != null ? objectSerializer : new ObjectSerializer();
			this.folderStructure = folderStructure != null ? folderStructure : new FolderStructure();
			this.storage = storage != null ? storage : new Storage(this.objectSerializer, this.folderStructure);
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

		public async Task Set(object rawKey, object value)
        {
            var key = ComputeKey( rawKey );
            CacheEntry cacheEntry = new CacheEntry ( null, value );

            cacheDictionary[CacheKey(key)] = cacheEntry;

            await this.storage.Write(key, cacheEntry, this.storeName);
        }

        public void Remove(object rawKey)
        {
			var key = ComputeKey(rawKey);
			cacheDictionary.Remove( CacheKey( key ), out _ );
            this.storage.Remove( key, this.storeName );
        }

		public async Task<(bool Found, object? Value)> TryGetValue( object rawKey )
		{
			var key = ComputeKey(rawKey);

			CacheEntry? cacheEntry;
			if (!cacheDictionary.TryGetValue( CacheKey( key ), out cacheEntry ))
			{
				cacheEntry = await this.storage.Read(key, this.storeName);
				if (cacheEntry == null)
					return (false, null);

				cacheDictionary.TryAdd( key, cacheEntry );
			}

			return (true, cacheEntry!.Value);
		}

		public async Task<T?> Get<T>(object rawKey)
        {
			var tuple = await TryGetValue(rawKey);
			return (T?) ( tuple.Found ? tuple.Value : null);
        }

        public bool Exists( object rawKey )
        {
			var key = ComputeKey(rawKey);

			if (cacheDictionary.ContainsKey( CacheKey( key ) )) 
				return true;

			return this.storage.Exists( key, this.storeName );
        }
    }
}
