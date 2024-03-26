using System;
using NUnit.Framework;


namespace NetMemCache.UnitTests
{
	[TestFixture]
	public class ConcurrencyTests
	{
		MemCache memCache = new MemCache( "ConcurrencyTests" );

		public ConcurrencyTests()
		{
			if (Directory.Exists( memCache.StoreName ) )
				Directory.Delete( memCache.StoreName, recursive: true);
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
				this.memCache.Remove( keys[i] );
			}
		}

		Task RunWriteTask( string key, Model m )
		{
			return this.memCache.Set( key, m );
		}
		Task<Model> RunReadTask( string key )
		{
			return this.memCache.Get<Model>( key );
		}
	}
}
