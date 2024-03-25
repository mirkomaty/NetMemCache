using BinaryRage.Interfaces;
using System.IO.Compression;
using System.Runtime.Serialization.Formatters.Binary;
using Newtonsoft.Json;
using System;
using System.Text;

namespace BinaryRage
{
	internal class ObjectSerializer : IObjectSerializer
	{
		public async Task<object?> Deserialize( byte[] array )
		{
			byte[] compressGZipData = await DecompressGZip(array);
			return ByteArrayToObject(compressGZipData);
		}

		public async Task<byte[]> SerializeAsync( StorageEntry storageEntry, Stream stream )
		{
			if (storageEntry.Value == null || storageEntry.Value.GetType() == typeof( string ) && ( (string) storageEntry.Value ).Length < 200)
			{
				using (var sw = new StreamWriter( stream, Encoding.UTF8 ))
				{
					sw.Write( (byte) 1 ); // compressed
					sw.Write( (byte) 1 ); // serialized
					await sw.WriteAsync( storageEntry.Type!.FullName );
					if (storageEntry == null)
					{ 
						//Hier muss man die Fälle als String oder einen Anderen Typen unterscheiden
						........
					}
					else
						await sw.WriteAsync( (string)storageEntry.Value );
				}
			}
			else
			{
				var json = JsonConvert.SerializeObject(storageEntry.Value);

				using (GZipStream gzipStream = new GZipStream( stream, CompressionMode.Compress, false ))
				{
					using (var sw = new StreamWriter( gzipStream, Encoding.UTF8 ))
					{
						sw.Write( (byte) 1 ); // compressed
						sw.Write( (byte) 1 ); // serialized
						await sw.WriteAsync( storageEntry.Type!.FullName );
						await sw.WriteAsync( json );
					}
				}
			}
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

			object obj2 = obj;

			string stringData;
			if (obj is string s)
			{
				stringData = s;
			}
			else
			{
				stringData = JsonConvert.SerializeObject( obj );
			}

			var typeStr = obj2.GetType().FullName;

				//BinaryFormatter formatter = new BinaryFormatter();
				MemoryStream ms = new MemoryStream();

			using (ms)
			{
				JsonConvert.SerializeObject(obj)
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
