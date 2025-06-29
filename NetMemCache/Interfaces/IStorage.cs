﻿using System;

namespace NetMemCache.Interfaces
{
	public interface IStorage
	{
		/// <summary>
		/// Writes data to a store
		/// </summary>
		/// <param name="key"></param>
		/// <param name="value">A byte array representing the data</param>
		/// <param name="store">The store</param>
		/// <returns></returns>
		Task Write( string key, CacheEntry cacheEntry, string store );

		/// <summary>
		/// Reads data from the store
		/// </summary>
		/// <param name="key">The key</param>
		/// <param name="store">The store</param>
		/// <returns></returns>
		Task<CacheEntry?> Read( string key, string store );

		/// <summary>
		/// Checks, if an entry exists in the store
		/// </summary>
		/// <param name="key">The key of the entry</param>
		/// <param name="store">The store</param>
		/// <returns></returns>
		bool Exists( string key, string store );

		/// <summary>
		/// Removes an entry from the store
		/// </summary>
		/// <param name="key">The key</param>
		/// <param name="store">The store</param>
		void Remove( string key, string store );

		/// <summary>
		/// Gets the file extension used in the storage.
		/// </summary>
		string Extension { get; }
	}
}
