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
            BinaryCache binaryCache = new BinaryCache( "bc_file" );

            public InsertTests()
            {
                if (Directory.Exists( binaryCache.StoreName ))
                    Directory.Delete( binaryCache.StoreName, recursive: true );
            }

            [Test]
            public async Task ShouldInsertAnObjectToStore()
            {
                var model = new Model{ Title ="title1", ThumbUrl="http://thumb.com/title1.jpg", Description="description1", Price=5.0F };
                await this.binaryCache.Set<Model>( "myModel", model );

                var result = await this.binaryCache.Get<Model>( "myModel" );

                Assert.That( model.Equals( result ) );
                this.binaryCache.Remove( "myModel" );
            }

			[Test]
			public async Task ShouldBeAbleToInsertAndRetrieveNull()
			{
				Model model = null;
				await this.binaryCache.Set<Model>( "nullModel", model );

				var result = await this.binaryCache.Get<Model>( "nullModel" );

				Assert.That( result == null );
				this.binaryCache.Remove( "nullModel" );
			}

			[Test]
            public async Task ShouldInsertAndGetKeysWithInvalidChars()
            {
                var model = new Model{Title ="title1", ThumbUrl="http://thumb.com/title1.jpg", Description="description1", Price=5.0F};
                await this.binaryCache.Set<Model>( "my:Model", model );

                var result = await this.binaryCache.Get<Model>("my:Model");

                Assert.That( model.Equals( result ) );
                this.binaryCache.Remove( "my:Model" );
                Assert.That( !this.binaryCache.Exists( "my:Model" ) );
            }

            [Test]
            public async Task ShouldInsertAListOfObjectsToStore()
            {
                var models = new List<Model> {
                    new Model{Title ="title1", ThumbUrl="http://thumb.com/title1.jpg", Description="description1", Price=5.0F},
                    new Model{Title ="title2", ThumbUrl="http://thumb.com/title2.jpg", Description="description2", Price=6.0F},
                    new Model{Title ="title3", ThumbUrl="http://thumb.com/title3.jpg", Description="description3", Price=7.0F},
                };

                await this.binaryCache.Set<List<Model>>( "myModels", models );

                var result = await this.binaryCache.Get<List<Model>>( "myModels" );

                CollectionAssert.AreEqual( models, result );
                this.binaryCache.Remove( "myModels" );
            }

            [Test]
            [Ignore( "Too much files" )]
            public async Task ShouldBeAbleToHandleHeavyLoad()
            {
                var dt = DateTime.Now;
                var count = 10000;
                for (int i = 0; i < count; i++)
                {
                    var model = new Model{Title ="title" + i, ThumbUrl=$"http://thumb.com/title{i}.jpg", Description=$"description{i}", Price=(float)i};
                    await this.binaryCache.Set<Model>( "myModel" + i, model );
                }

                for (int i = 0; i < count; i++)
                {
                    var model = new Model { Title = "title" + i, ThumbUrl = $"http://thumb.com/title{i}.jpg", Description = $"description{i}", Price = (float) i };
                    var result = await this.binaryCache.Get<Model>("myModel" + i);

                    Assert.That( model.Equals( result ) );
                }

                var ts = DateTime.Now - dt;
                Debug.WriteLine( "Time Write and Read: " + ts );

                for (int i = 0; i < count; i++)
                {
                    this.binaryCache.Remove( "myModel" + i );
                }

                ts = DateTime.Now - dt;
                Debug.WriteLine( "Time with Delete: " + ts );
            }
        }
    }
}
