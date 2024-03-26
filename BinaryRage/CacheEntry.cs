using System;

namespace BinaryRage
{
	public class CacheEntry
	{
		public CacheEntry( DateTime? expiryDate, object? value, Type type )
		{
			ExpiryDate = expiryDate;
			Value = value;
			Type = type;
		}

		public CacheEntry(StorageEntry storageEntry)
		{
			ExpiryDate = storageEntry.ExpiryDate;
			Value = storageEntry.Value;
			Type = storageEntry.Type;
		}

		public DateTime? ExpiryDate { get; }
		public object? Value { get; set; }
		public Type Type { get; set; }
	}
}