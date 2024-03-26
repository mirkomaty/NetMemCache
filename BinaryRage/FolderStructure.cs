using BinaryRage.Interfaces;
using System;

namespace BinaryRage
{
	public class FolderStructure : IFolderStructure
	{
		public IEnumerable<string> Generate( string key, string store )
		{
			var hashed = key.GetHashCode().ToString( "X8" ).Substring( 0, 4 );
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
	}
}
