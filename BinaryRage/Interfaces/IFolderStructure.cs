namespace NetMemCache.Interfaces
{
	public interface IFolderStructure
	{
		public IEnumerable<string> Generate( string key, string storeName );
	}
}
