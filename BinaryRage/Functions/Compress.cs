using System.IO;
using System.IO.Compression;

namespace BinaryRage.Functions
{
	public static class Compress
	{

		//Compress bytes
		public async static Task<byte[]> CompressGZip(byte[] raw)
		{
			using (MemoryStream memory = new MemoryStream())
			{
				using (GZipStream gzip = new GZipStream(memory, CompressionMode.Compress, false))
				{
					await gzip.WriteAsync(raw, 0, raw.Length);
				}

				return memory.ToArray();
			}
		}

		//Decompress bytes
		public async static Task<byte[]> DecompressGZip( byte[] gzip )
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
	}
}
