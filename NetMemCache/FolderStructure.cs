using NetMemCache.Interfaces;
using System;
using System.Text;

namespace NetMemCache
{
	public class FolderStructure : IFolderStructure
	{
		public IEnumerable<string> Generate( string key, string store )
		{
			var hashed = GetHash(key).ToString( "X8" ).Substring( 0, 4 );
			yield return store;

			foreach (var folder in SplitKey( hashed, 2 ))
				yield return folder;
		}

		private IEnumerable<string> SplitKey( string str, int maxLength )
		{
			if (maxLength < 1)
				maxLength = 1;

			for (int index = 0; index < str.Length; index += maxLength)
			{
				yield return str.Substring( index, Math.Min( maxLength, str.Length - index ) );
			}
		}

        private static int GetHash( string s )
        {
            Byte[] barr = Encoding.UTF8.GetBytes(s);
            int hash = -1;
            int l = barr.Length;
            for (int i = 0; i < l; i++)
            {
                byte b = barr[i];
                hash = ( hash << 5 ) - hash + b;
            }
            return hash;
        }        
    }
}
