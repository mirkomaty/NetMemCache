namespace BinaryRage.Interfaces
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

		/// <summary>
		/// Serializes an object to a unique key string
		/// </summary>
		/// <param name="rawKey"></param>
		/// <returns></returns>
		/// <remarks>The result does not guarantee that any requirements for the string are met. 
		/// If the key is to be used for names in the file system, it should be encoded accordingly.
		/// </remarks>
		string SerializeKey( object rawKey );
	}
}
