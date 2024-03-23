using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BinaryRage
{
	public interface IFolderStructure
	{
		public IEnumerable<string> Generate( string key, string storeName );
	}
}
