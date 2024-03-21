namespace BinaryRage
{
	internal static class Storage
	{
		private const string DB_EXTENSION = ".odb";
		static object lockObject = new object();

		private static string createDirectoriesBasedOnKeyAndFilelocation(string key, string filelocation)
		{
			lock (lockObject)
			{
				string pathSoFar = "";
				foreach (var folder in GetFolders( ComputeHash( key ), filelocation ))
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

		public static void Remove(string key, string fileLocation)
		{
			lock(lockObject)
			{
				File.Delete( GetExactFileLocation( key, fileLocation ) );
			}
		}

		public async static Task WriteToStorage(string key, byte[] value, string filelocation)
		{
			//create folders
			string dirstructure = createDirectoriesBasedOnKeyAndFilelocation(key, filelocation);

			//Write the file to it's location
            await File.WriteAllBytesAsync(CombinePathAndKey(dirstructure, key), value);
		}

		public async static Task<byte[]> GetFromStorage(string key, string filelocation)
		{
			return await File.ReadAllBytesAsync(GetExactFileLocation(key, filelocation));
		}

		public static bool ExistingStorageCheck(string key, string filelocation)
		{
			return File.Exists(GetExactFileLocation(key, filelocation));
		}

		static string ComputeHash(string key)
		{
			return key.GetHashCode().ToString( "X8" ).Substring( 0, 4 );
		}

		private static string GetExactFileLocation( string key, string filelocation )
		{
			return CombinePathAndKey(
				path: Path.Combine( GetFolders( ComputeHash( key ), filelocation ).ToArray() ),
				key: key );
		}

		private static string CombinePathAndKey(string path, string key)
		{
			return Path.Combine(path, key + DB_EXTENSION);
		}

		private static IEnumerable<string> GetFolders(string key, string filelocation)
		{
			lock(lockObject)
			{
				return InternalGetFolders( key, filelocation );
			}
		}

		private static IEnumerable<string> InternalGetFolders( string key, string filelocation )
		{
			yield return filelocation;
			foreach (var folder in Key.Splitkey( key ))
				yield return folder;
		}
	}
}