namespace BinaryRage
{
	public class StorageEntry
	{
		public StorageEntry( CacheEntry cacheEntry, Stream stream )
		{
			Type = cacheEntry.Type;
			Value = cacheEntry.Value;
			ExpiryDate = cacheEntry.ExpiryDate;
			Stream = stream;
		}

		public StorageEntry(bool isCompressed, bool isSerialized, Type type, DateTime? expiryDate )
		{
			IsCompressed = isCompressed;
			IsSerialized = isSerialized;
			Type = type;
			ExpiryDate = expiryDate;
		}

		public bool IsCompressed { get; set; }
	
		public bool IsSerialized { get; set; }
		public Type Type { get; set; }
		public DateTime? ExpiryDate { get; set; }
		public Stream Stream { get; set; } = new MemoryStream();
		public object? Value { get; set; }
	}
}
