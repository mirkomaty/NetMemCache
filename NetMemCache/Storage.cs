using NetMemCache.Interfaces;

namespace NetMemCache
{
	public class Storage : IStorage
	{
		private static readonly string DB_EXTENSION = ".nmc";
		private readonly IFolderStructure folderStructure;
		private readonly IObjectSerializer objectSerializer;
		object lockObject = new object();

		public Storage( IObjectSerializer objectSerializer, IFolderStructure? folderStructure = null )
		{
			this.folderStructure = folderStructure == null ? new FolderStructure() : folderStructure;
			this.objectSerializer = objectSerializer;
		}

		public string Extension => DB_EXTENSION;

		public void Remove(string key, string fileLocation)
		{
			lock(lockObject)
			{
				File.Delete( GetExactFileLocation( key, fileLocation ) );
			}
		}

		/// <inheritdoc/>
		public async Task Write(string key, CacheEntry cacheEntry, string store)
		{				
			//create folders
			string dirstructure = CreateDirectoriesBasedOnKeyAndFilelocation(key, store);

			using (var fileStream = File.OpenWrite( CombinePathAndKey( dirstructure, key ) ))
			{
				StorageEntry storageEntry = new StorageEntry(cacheEntry, fileStream);
				await this.objectSerializer.SerializeAsync( storageEntry, fileStream );
			}
			//Write the file to it's location
		}

		/// <inheritdoc/>
		public async Task<CacheEntry?> Read(string key, string store)
		{
			StorageEntry storageEntry;
			string path = GetExactFileLocation( key, store );
			if (!File.Exists( path ))
				return null;

			using (var stream = File.OpenRead( path ))
			{
				storageEntry = await this.objectSerializer.DeserializeAsync(stream);
			}

			return new CacheEntry( storageEntry );
		}

		/// <inheritdoc/>
		public bool Exists(string key, string store)
		{
			return File.Exists(GetExactFileLocation(key, store));
		}

		private string CreateDirectoriesBasedOnKeyAndFilelocation( string key, string store )
		{
			lock (lockObject)
			{
				string pathSoFar = "";
				foreach (var folder in GetFolders( key, store ))
				{
					try
					{
						pathSoFar = Path.Combine( pathSoFar, folder );
						if (!Directory.Exists( pathSoFar ))
							Directory.CreateDirectory( pathSoFar );
					}
					catch (Exception)
					{

					}
				}
				return pathSoFar;
			}
		}

		private string GetExactFileLocation( string key, string store )
		{
			return CombinePathAndKey(
				path: Path.Combine( GetFolders( key, store ).ToArray() ),
				key: key );
		}

		private IEnumerable<string> GetFolders( string key, string store )
		{
			lock (lockObject)
			{
				return this.folderStructure.Generate( key, store );
			}
		}


		private string CombinePathAndKey(string path, string key)
		{
			return Path.Combine(path, key + DB_EXTENSION);
		}
	}
}