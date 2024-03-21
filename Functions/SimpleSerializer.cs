using System;
using Newtonsoft.Json;

namespace BinaryRage.Functions
{
	internal class SimpleSerializer
	{
		public static string Serialize<T>(T myobj)
		{
			return JsonConvert.SerializeObject(myobj);
		}		
	}
}