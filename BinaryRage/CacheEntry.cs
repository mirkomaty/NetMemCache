using System;

namespace BinaryRage
{
	public class CacheEntry
	{
		public CacheEntry( DateTime? expiryDate, object? value )
		{
			ExpiryDate = expiryDate;
			Value = value;
		}

		public CacheEntry(StorageEntry storageEntry)
		{
			ExpiryDate = storageEntry.ExpiryDate;
			Value = storageEntry.Value;
		}

		public DateTime? ExpiryDate { get; }
		public object? Value { get; set; }
	}
}