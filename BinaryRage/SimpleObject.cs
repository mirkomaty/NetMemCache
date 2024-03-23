using System;

namespace BinaryRage
{
	internal class SimpleObject
	{
		public SimpleObject( string key, object? value, string store )
		{
			Key = key;
			Store = store;
			Value = value;
		}

		public string Key { get; set; }
		public object? Value { get; set; }
		public string Store { get; set; }
	}
}
