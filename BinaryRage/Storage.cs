namespace BinaryRage
{
	public class Storage : IStorage
	{
		private const string DB_EXTENSION = ".odb";
		private readonly IFolderStructure folderStructure;
		object lockObject = new object();

		public Storage( IFolderStructure? folderStructure = null )
		{
			this.folderStructure = folderStructure == null ? new FolderStructure() : folderStructure;
		}

		public void Remove(string key, string fileLocation)
		{
			lock(lockObject)
			{
				File.Delete( GetExactFileLocation( key, fileLocation ) );
			}
		}

		/// <inheritdoc/>
		public async Task Write(string key, byte[] value, string store)
		{
			//create folders
			string dirstructure = CreateDirectoriesBasedOnKeyAndFilelocation(key, store);

			//Write the file to it's location
            await File.WriteAllBytesAsync(CombinePathAndKey(dirstructure, key), value);
		}

		/// <inheritdoc/>
		public async Task<byte[]> Read(string key, string store)
		{
			return await File.ReadAllBytesAsync(GetExactFileLocation(key, store));
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