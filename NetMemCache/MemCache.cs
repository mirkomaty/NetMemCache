﻿using System;
using System.Collections.Concurrent;
using NetMemCache.Interfaces;

namespace NetMemCache
{
	/// <summary>
	/// Represents a KeyValue store
	/// </summary>
    public class MemCache
    {
		private static ConcurrentDictionary<string, CacheEntry> cacheDictionary = new ConcurrentDictionary<string, CacheEntry>();
		private readonly IStorage storage;
		private readonly string storeName;
		private readonly IObjectSerializer objectSerializer;
		private readonly IFolderStructure folderStructure;
		private readonly IKeyHandler keyHandler;

		public string StoreName => this.storeName;

		/// <summary>
		/// This is for test purposes only. Don't set this value to true.
		/// </summary>
		public bool DisableDictionary { get; set; } = false;

		/// <summary>
		/// Constructs a Cache object.
		/// </summary>
		/// <param name="storeName"></param>
		/// <param name="storage"></param>
		/// <param name="folderStructure"></param>
		/// <param name="objectSerializer"></param>
		/// <param name="keyHandler"></param>
		public MemCache(
			string storeName, 
			IStorage? storage = null, 
			IFolderStructure? folderStructure = null, 
			IObjectSerializer? objectSerializer = null, 
			IKeyHandler? keyHandler = null
		)
		{
			this.keyHandler = keyHandler ?? new KeyHandler();
			var arr = storeName.Split(Path.DirectorySeparatorChar);
			for (int i = 0; i < arr.Length; i++)
			{
				arr[i] = this.keyHandler.NormalizeKey( arr[i], true );
			}
			this.storeName = String.Join( Path.DirectorySeparatorChar, arr );
			this.objectSerializer = objectSerializer ?? new ObjectSerializer();
			this.folderStructure = folderStructure ?? new FolderStructure();
			this.storage = storage ?? new Storage(this.objectSerializer, this.folderStructure);
		}

		private string CacheKey( string key ) => this.storeName + key;

		/// <summary>
		/// Writes an object to the cache.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="rawKey">An object representing the key.</param>
		/// <param name="value">The value to be stored. May be null.</param>
		/// <returns></returns>
		public async Task Set<T>( object rawKey, T value )
		{
			var key = this.keyHandler.ComputeKey( rawKey );
			CacheEntry cacheEntry = new CacheEntry ( null, value, typeof(T) );

			if (!DisableDictionary)
				cacheDictionary[CacheKey( key )] = cacheEntry;

			await this.storage.Write( key, cacheEntry, this.storeName );
		}

		public async Task Set<T>( object rawKey, T value, int milliseconds )
		{
			var ts = TimeSpan.FromMilliseconds(milliseconds);
			await Set<T>( rawKey, value, ts );
		}

		public async Task Set<T>( object rawKey, T value, TimeSpan timeSpan )
		{
			var key = this.keyHandler.ComputeKey( rawKey );
			CacheEntry cacheEntry = new CacheEntry ( DateTime.Now + timeSpan, value, typeof(T) );

			if (!DisableDictionary)
				cacheDictionary[CacheKey( key )] = cacheEntry;

			await this.storage.Write( key, cacheEntry, this.storeName );
		}

		/// <summary>
		/// Removes an object from the cache.
		/// </summary>
		/// <param name="rawKey">An object representing the key.</param>
		public void Remove(object rawKey)
        {
			var key = this.keyHandler.ComputeKey(rawKey);
			cacheDictionary.Remove( CacheKey( key ), out _ );
            this.storage.Remove( key, this.storeName );
        }

		/// <summary>
		/// Tries to read an object from the cache.
		/// </summary>
		/// <param name="rawKey">An object representing the key.</param>
		/// <returns>A tuple (Found, Object). Found is true, if the object could be found. Value contains the object in this case. Found is false, if the object could not be found.</returns>
		public async Task<(bool Found, object? Value)> TryGetValue( object rawKey )
		{
			var key = this.keyHandler.ComputeKey(rawKey);
			var now = DateTime.Now;

			CacheEntry? cacheEntry;
			if (!cacheDictionary.TryGetValue( CacheKey( key ), out cacheEntry ))
			{
				cacheEntry = await this.storage.Read(key, this.storeName);
				if (cacheEntry == null)
					return (false, null);

				if (cacheEntry.ExpiryDate <= now)
					cacheDictionary.TryAdd( key, cacheEntry );
			}

			if (cacheEntry.ExpiryDate <= now)
			{
				Remove( rawKey );
				return (false, null);
			}

			return (true, cacheEntry!.Value);
		}

		/// <summary>
		/// Gets an object from the cache.
		/// </summary>
		/// <typeparam name="T">The object type to be found.</typeparam>
		/// <param name="rawKey">An object representing the key.</param>
		/// <returns>Returns the object or null, if the object could not be found.</returns>
		/// <remarks>This method can't distinquish between a stored value of null and object not found. If you need to distinguish these conditions, use TryGetValue.</remarks>
		public async Task<T?> Get<T>(object rawKey)
        {
			var tuple = await TryGetValue(rawKey);
			return (T?) ( tuple.Found ? tuple.Value : null);
        }

		/// <summary>
		/// Determines, if a given object exists.
		/// </summary>
		/// <param name="rawKey">An object representing the key.</param>
		/// <returns></returns>
		public bool Exists( object rawKey )
        {
			var key = this.keyHandler.ComputeKey(rawKey);

			if (cacheDictionary.ContainsKey( CacheKey( key ) )) 
				return true;

			return this.storage.Exists( key, this.storeName );
        }

		/// <summary>
		/// Removed expired entries from the memory cache and from disk.
		/// </summary>
		public void RemoveExpired()
		{
			var now = DateTime.Now;

			foreach (var key in cacheDictionary.Keys)
			{
				var entry = cacheDictionary[key];
				if (entry.ExpiryDate.HasValue && entry.ExpiryDate <= now)
				{
					cacheDictionary.Remove( key, out _ );

					if (this.storage.Exists( key, this.storeName ))
						this.storage.Remove( key, this.storeName );
				}
			}

			var baseFolder = this.storeName;
			RemoveExpired( baseFolder, now );
		}

		private void RemoveExpired( string path, DateTime now )
		{
			var dirs = Directory.GetDirectories(path);
			foreach (var dir in dirs)
			{
				RemoveExpired( dir, now );
			}

			var fileNames = Directory.GetFiles(path, $"*{this.storage.Extension}");
			foreach (var fileName in fileNames)
			{
				DateTime? expiryDate;
				using (var stream = File.OpenRead(fileName))
				{
					expiryDate = this.objectSerializer.GetExpiryDate( stream );
				}

				if (expiryDate.HasValue && expiryDate <= now)
					File.Delete( fileName );
			}
		}
    }
}
