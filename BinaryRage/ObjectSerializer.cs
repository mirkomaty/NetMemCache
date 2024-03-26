using BinaryRage.Interfaces;
using System.IO.Compression;
using System.Runtime.Serialization.Formatters.Binary;
using Newtonsoft.Json;
using System;
using System.Text;
using System.Reflection;

namespace BinaryRage
{
	internal class ObjectSerializer : IObjectSerializer
	{
		static readonly int headerLength = 2 * sizeof(byte) + sizeof(long);

		///<inheritdoc/>
		public async Task<StorageEntry> DeserializeAsync( Stream stream )
		{
			var headerBytes = new byte[headerLength];
			var bytesRead = await stream.ReadAsync( headerBytes, 0, headerLength );
			
			if (bytesRead < headerLength)
				throw new Exception( "Wrong file format: can't read header" );

			var isCompressed = headerBytes[0] != 0;
			var isSerialized = headerBytes[1] != 0;
			var ticks = BitConverter.ToInt64(headerBytes, 2);
			string typeStr;
			string storedString;

			Stream innerStream = isCompressed ? new GZipStream(stream, CompressionMode.Decompress) : stream;
			using (innerStream)
			{
				using (var sr = new StreamReader( innerStream ))
				{
					var ts = await sr.ReadLineAsync();
					if (ts == null)
						throw new Exception( "Can't read type information from the stream" );
					typeStr = ts;

					storedString = await sr.ReadToEndAsync();
				}
			}

			StorageEntry result = new StorageEntry();
			result.IsCompressed = isCompressed;
			result.IsSerialized = isSerialized;
			result.ExpiryDate = ticks != 0L ? new DateTime( ticks ) : null;
			result.Type = Type.GetType(typeStr);

			if (isSerialized)
			{
				if (result.Type == null)
					throw new Exception( $"Can't deserialize data because the type '{typeStr}' can't be loaded" );
				result.Value = JsonConvert.DeserializeObject( storedString, result.Type );
			}
			else
			{
				result.Value = storedString;
			}

			return result;
		}

		///<inheritdoc/>
		public async Task SerializeAsync( StorageEntry storageEntry, Stream stream )
		{
			string? valueToWrite = null;

			if (storageEntry.Value != null)
			{
				if (storageEntry.Value is string s)
				{
					storageEntry.IsSerialized = false;
					valueToWrite = s;
				}
				else
				{
					valueToWrite = JsonConvert.SerializeObject( storageEntry.Value );
					storageEntry.IsSerialized = true;
				}
			}

			// Nulltests for type follow later
			storageEntry.IsCompressed = valueToWrite != null && valueToWrite.Length + storageEntry.Type.FullName.Length > 200;

			var headerBytes = new byte[headerLength];
			headerBytes[0] = storageEntry.IsCompressed ? (byte) 1 : (byte) 0;
			headerBytes[1] = storageEntry.IsSerialized ? (byte) 1 : (byte) 0;
			var ticks = storageEntry.ExpiryDate.HasValue ? storageEntry.ExpiryDate.Value.Ticks : 0L;
			var tickArray = BitConverter.GetBytes( ticks );
			for (int i = 0; i < tickArray.Length; i++)
			{
				headerBytes[i + 2] = tickArray[i];
			}

			stream.Write( headerBytes, 0, headerBytes.Length );

			var innerStream = storageEntry.IsCompressed ? new GZipStream(stream, CompressionMode.Compress) : stream;
			using (innerStream)
			{
				using (var sw = new StreamWriter( stream, Encoding.UTF8 ))
				{
					Type t = storageEntry.Type!;
					await sw.WriteLineAsync( t.FullName + "," +  t.Assembly.FullName );
					await sw.WriteAsync( valueToWrite );
				}
			}
		}	

		///<inheritdoc/>
		public string SerializeKey( object rawKey )
		{
			return JsonConvert.SerializeObject( rawKey );
		}

	}
}
