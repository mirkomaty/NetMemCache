using BinaryRage.Interfaces;
using System.IO.Compression;
using System.Runtime.Serialization.Formatters.Binary;
using Newtonsoft.Json;

namespace BinaryRage
{
	internal class ObjectSerializer : IObjectSerializer
	{
		public async Task<object?> Deserialize( byte[] array )
		{
			byte[] compressGZipData = await DecompressGZip(array);
			return ByteArrayToObject(compressGZipData);
		}

		public async Task<byte[]> Serialize( object? obj )
		{
			return await CompressGZip( ObjectToByteArray( obj ) );
		}

		async static Task<byte[]> DecompressGZip( byte[] gzip )
		{
			using (GZipStream stream = new GZipStream( new MemoryStream( gzip ), CompressionMode.Decompress ))
			{
				using (MemoryStream memory = new MemoryStream())
				{
					await stream.CopyToAsync( memory );
					return memory.ToArray();
				}
			}
		}

		async static Task<byte[]> CompressGZip( byte[] raw )
		{
			using (MemoryStream memory = new MemoryStream())
			{
				using (GZipStream gzip = new GZipStream( memory, CompressionMode.Compress, false ))
				{
					await gzip.WriteAsync( raw, 0, raw.Length );
				}

				return memory.ToArray();
			}
		}

		public byte[] ObjectToByteArray( object? obj )
		{
			if (obj == null)
				return new byte[0];

			BinaryFormatter formatter = new BinaryFormatter();
			MemoryStream ms = new MemoryStream();

			using (ms)
			{

				formatter.Serialize( ms, obj );
			}

			return ms.ToArray();
		}

		//Convert BytesArray to object
		public Object? ByteArrayToObject( byte[] arrBytes )
		{
			if (arrBytes == null || arrBytes.Length == 0)
				return null;

			MemoryStream memStream = new MemoryStream();
			BinaryFormatter binForm = new BinaryFormatter();
			memStream.Write( arrBytes, 0, arrBytes.Length );
			memStream.Seek( 0, SeekOrigin.Begin );
			return (Object?) binForm.Deserialize( memStream );
		}

		///<inheritdoc/>
		public string SerializeKey( object rawKey )
		{
			return JsonConvert.SerializeObject( rawKey );
		}

	}
}
