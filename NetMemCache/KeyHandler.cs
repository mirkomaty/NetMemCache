using NetMemCache.Interfaces;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetMemCache
{
    internal class KeyHandler : IKeyHandler
	{
		static readonly char[] invalidPath = Path.GetInvalidPathChars();
		static readonly char[] invalidFile = Path.GetInvalidFileNameChars();

		public string NormalizeKey( string key, bool isPath )
		{
			StringBuilder sb = new StringBuilder();
			foreach (var c in key)
			{
				if (( isPath ? invalidPath : invalidFile ).Contains( c ))
				{
					var bytes = Encoding.UTF8.GetBytes( new[] { c } );
					foreach (byte b in bytes)
					{
						sb.Append( b.ToString( "X2" ) );
					}
				}
				else
				{
					sb.Append( c );
				}
			}

			return sb.ToString();
		}

		public string ComputeKey( object rawKey )
		{
			if (rawKey == null)
				throw new ArgumentNullException( nameof( rawKey ) );

			if (rawKey is string)
				return NormalizeKey( (string) rawKey, false );

			return NormalizeKey( SerializeKey( rawKey ), false );
		}

		/// <summary>
		/// Serializes an object to a unique key string
		/// </summary>
		/// <param name="rawKey"></param>
		/// <returns></returns>
		/// <remarks>The result does not guarantee that any requirements for the string are met. 
		/// If the key is to be used for names in the file system, it should be encoded accordingly.
		/// </remarks>
		string SerializeKey( object rawKey )
		{
			return JsonConvert.SerializeObject( rawKey );
		}
	}
}
