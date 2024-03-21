using System;
using NUnit.Framework;


namespace BinaryRage.UnitTests
{
	[TestFixture]
	public class WaitForCompletionTests
	{
		const string DB_NAME = "WaitForCompletionTests";

		[SetUp]
		public void Setup()
		{
			if (Directory.Exists(DB_NAME))
				Directory.Delete(DB_NAME, recursive: true);
		}

		[Test]
		public async Task ShouldWaitForIOCompletionWhenAsked()
		{
			var m = new Model { Description = "foobar" };
			for (int i = 0; i < 10; i++)
				await DB.Insert<Model>("key" + i, m, DB_NAME);

			var readObjects = new Dictionary<string, Model>();
			for (int i = 0; i < 10; i++)
				readObjects.Add("key" + i, await DB.Get<Model>("key" + i, DB_NAME));

			Assert.That( m.Description.Equals( readObjects["key0"].Description ) );
			Assert.That( m.Description.Equals( readObjects["key9"].Description ) );
		}
	}
}
