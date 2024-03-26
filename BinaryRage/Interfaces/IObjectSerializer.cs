namespace NetMemCache.Interfaces
{
	/// <summary>
	/// Interface for Serializers, which can convert objects to byte streams
	/// and vice versa.
	/// </summary>
	/// <remarks>The original type of the object is retained.</remarks>
	public interface IObjectSerializer
	{
		/// <summary>
		/// Serializes an object into a byte array
		/// </summary>
		/// <param name="obj"></param>
		/// <returns></returns>
		Task SerializeAsync( StorageEntry storageEntry, Stream stream );

		/// <summary>
		/// Deserializes an object from a byte array
		/// </summary>
		/// <param name="array"></param>
		/// <returns></returns>
		Task<StorageEntry> DeserializeAsync( Stream stream );

	}
}
