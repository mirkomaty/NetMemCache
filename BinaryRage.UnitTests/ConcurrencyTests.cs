using System;
using NUnit.Framework;


namespace BinaryRage.UnitTests
{
	[TestFixture]
	public class ConcurrencyTests
	{
		const string DB_NAME = "ConcurrencyTests";

		[SetUp]
		public void Setup()
		{
			if (Directory.Exists(DB_NAME))
				Directory.Delete(DB_NAME, recursive: true);
		}

		[Test]
		public void ConcurrentReadAndWriteShouldWork()
		{
			var m = new Model { Description = "foobar" };
			int count = 100;
			var tasks = new Task[count];
			var keys = new string[count];
			
			for (int i = 0; i < count; i++)
			{
				keys[i] = Guid.NewGuid().ToString();
			}

			for (int i = 0; i < count; i++)
			{
				tasks[i] = RunWriteTask( keys[i], m );
			}

			Task.WaitAll( tasks );

			var readTasks = new Task<Model>[count];

			for (int i = 0; i < count; i++)
			{
				readTasks[i] = RunReadTask( keys[i] );
			}

			Task.WaitAll( readTasks );

			for (int i = 0; i < count; i++)
			{
				Assert.That( m.Description.Equals( readTasks[i].GetAwaiter().GetResult().Description ) );
				DB.Remove( keys[i], DB_NAME );
			}
		}

		static Task RunWriteTask( string key, Model m )
		{
			return DB.Insert<Model>( key, m, DB_NAME );
		}
		static Task<Model> RunReadTask( string key )
		{
			return DB.Get<Model>( key, DB_NAME );
		}

	}
}
