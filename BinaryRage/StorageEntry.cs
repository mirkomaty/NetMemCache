namespace BinaryRage
{
	public class StorageEntry
	{
		public bool IsCompressed;
		public bool IsSerialized;
		public Type? Type;
		public DateTime? ExpiryDate;
		public Stream Stream = new MemoryStream();
		public object? Value;
	}
}
