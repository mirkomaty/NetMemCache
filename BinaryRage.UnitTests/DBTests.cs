using NUnit.Framework;
using NUnit.Framework.Legacy;
using System.Diagnostics;

namespace BinaryRage.UnitTests
{
    public class DBTests
    {
        [TestFixture]
        public class InsertTests
        {
            [Test]
            public async Task ShouldInsertAnObjectToStore()
            { 
                var model = new Model{Title ="title1", ThumbUrl="http://thumb.com/title1.jpg", Description="description1", Price=5.0F};
                await BinaryRage.DB.Insert<Model>("myModel", model, "dbfile");

                var result = await BinaryRage.DB.Get<Model>("myModel", "dbfile");
               
                Assert.That(model.Equals(result));
                BinaryRage.DB.Remove("myModel", "dbfile");
            }

            [Test]
            public async Task ShouldInsertAListOfObjectsToStore()
            {
                var models = new List<Model> { 
                    new Model{Title ="title1", ThumbUrl="http://thumb.com/title1.jpg", Description="description1", Price=5.0F},
                    new Model{Title ="title2", ThumbUrl="http://thumb.com/title2.jpg", Description="description2", Price=6.0F},
                    new Model{Title ="title3", ThumbUrl="http://thumb.com/title3.jpg", Description="description3", Price=7.0F},
                };

                await BinaryRage.DB.Insert<List<Model>>("myModels", models, "dbfile");

                var result = await BinaryRage.DB.Get<List<Model>>("myModels", "dbfile");

                CollectionAssert.AreEqual(models, result);
                BinaryRage.DB.Remove("myModels", "dbfile");
            }

			[Test]
            [Ignore("Too much files")]
			public async Task ShouldBeAbleToHandleHeavyLoad()
			{
                var dt = DateTime.Now;
                var count = 10000;
                for (int i = 0; i < count; i++)
                {
                    var model = new Model{Title ="title" + i, ThumbUrl=$"http://thumb.com/title{i}.jpg", Description=$"description{i}", Price=(float)i};
                    await BinaryRage.DB.Insert<Model>( "myModel" + i, model, "dbfile" );
                }

                for (int i = 0; i < count; i++)
                {
					var model = new Model { Title = "title" + i, ThumbUrl = $"http://thumb.com/title{i}.jpg", Description = $"description{i}", Price = (float) i };
					var result = await BinaryRage.DB.Get<Model>("myModel" + i, "dbfile");

                    Assert.That( model.Equals( result ) );
                }

				var ts = DateTime.Now - dt;
				Debug.WriteLine( "Time Write and Read: " + ts );

                for (int i = 0; i < count; i++)
                {
					BinaryRage.DB.Remove( "myModel" + i, "dbfile" );
				}

				ts = DateTime.Now - dt;
				Debug.WriteLine( "Time with Delete: " + ts );
			}
		}
    }
}
