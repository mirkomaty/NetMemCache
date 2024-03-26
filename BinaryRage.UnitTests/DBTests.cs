using NUnit.Framework;
using NUnit.Framework.Legacy;
using System.Diagnostics;
using System.Text;

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
            public async Task ShouldInsertAndRetrieveASmallObject()
            {
                var model = new Model{ Title = "Test" };
                await this.binaryCache.Set( "myModel", model );

                var result = await this.binaryCache.Get<Model>( "myModel" );

                Assert.That( model.Equals( result ) );
                this.binaryCache.Remove( "myModel" );
            }

			[Test]
			public async Task ShouldInsertAndRetrieveABigObject()
			{
				var model = new Model{ Title ="title1", ThumbUrl="http://thumb.com/title1.jpg", Description="Lorem ipsum dolor sit amet, consectetuer adipiscing elit. Aenean commodo ligula eget dolor. Aenean massa. Cum sociis natoque penatibus et magnis dis parturient montes, nascetur ridiculus mus. Donec quam felis, ultricies nec, pellentesque eu, pretium quis, sem. Nulla consequat massa quis enim. Donec pede justo, fringilla vel, aliquet nec, vulputate eget, arcu. In enim justo, rhoncus ut, imperdiet a, venenatis vitae, justo. Nullam dictum felis eu pede mollis pretium. Integer tincidunt. Cras dapibus. Vivamus elementum semper nisi. Aenean vulputate eleifend tellus. Aenean leo ligula, porttitor eu, consequat vitae, eleifend ac, enim. Aliquam lorem ante, dapibus in, viverra quis, feugiat a, tellus. Phasellus viverra nulla ut metus varius laoreet. Quisque rutrum. Aenean imperdiet. Etiam ultricies nisi vel augue. Curabitur ullamcorper ultricies nisi. Nam eget dui. Etiam rhoncus. Maecenas tempus, tellus eget condimentum rhoncus, sem quam semper libero, sit amet adipiscing sem neque sed ipsum. Nam quam nunc, blandit vel, luctus pulvinar, hendrerit id, lorem. Maecenas nec odio et ante tincidunt tempus. Donec vitae sapien ut libero venenatis faucibus. Nullam quis ante. Etiam sit amet orci eget eros faucibus tincidunt. Duis leo. Sed fringilla mauris sit amet nibh. Donec sodales sagittis magna. Sed consequat, leo eget bibendum sodales, augue velit cursus nunc,", Price=5.0F };
				await this.binaryCache.Set( "myModel", model );

				var result = await this.binaryCache.Get<Model>( "myModel" );

				Assert.That( model.Equals( result ) );
				this.binaryCache.Remove( "myModel" );
			}


			[Test]
            public async Task ShouldBeAbleToInsertAndRetrieveNull()
            {
                Model model = null;
                await this.binaryCache.Set( "nullModel", model );

                var result = await this.binaryCache.Get<Model>( "nullModel" );

                Assert.That( result == null );
                this.binaryCache.Remove( "nullModel" );
            }

			[Test]
			public async Task ShouldReturnCorrectTypeIfNull()
			{
				await this.binaryCache.Set<Model>( "myModelTgvNull", null );

				Model result = await this.binaryCache.Get<Model>( "myModelTgvNull" );

				Assert.That( result == null );
				this.binaryCache.Remove( "myModelTgvNull" );
			}

			[Test]
			public void ShouldSupportPaths()
			{
                string[] segments = new string[]{"My", "Path"};
                var bcPath = Path.Combine( segments );

				var folderStructure = new FolderStructure();
                var result = Path.Combine( folderStructure.Generate( "test", bcPath ).ToArray() );
                Assert.That( result.StartsWith( bcPath ) );
                var resultSplit = result.Split(Path.DirectorySeparatorChar);
                Assert.That( resultSplit.Length == 4 );
			}

			[Test]
			public async Task GetShouldReturnNullIfNotFound()
			{
				var result = await this.binaryCache.Get<Model>( "notFoundModel" );

				Assert.That( result == null );
			}

			[Test]
			public async Task TryGetValueShouldReturnFalseIfNotFound()
			{
				var result = await this.binaryCache.TryGetValue( "notFoundModel" );

				Assert.That( !result.Found );
			}

			[Test]
			public async Task TryGetValueShouldRetrieveObject()
			{
				var model = new Model{ Title = "Test" };
				await this.binaryCache.Set<Model>( "myModelTgv", model );

				var result = await this.binaryCache.TryGetValue( "myModelTgv" );

                Assert.That( result.Found );
				Assert.That( model.Equals( result.Value ) );
				this.binaryCache.Remove( "myModelTgv" );
			}

			[Test]
            public async Task ShouldInsertAndGetKeysWithInvalidChars()
            {
                var model = new Model{Title ="title1", ThumbUrl="http://thumb.com/title1.jpg", Description="description1", Price=5.0F};
                await this.binaryCache.Set( "my:Model", model );

                var result = await this.binaryCache.Get<Model>("my:Model");

                Assert.That( model.Equals( result ) );
                this.binaryCache.Remove( "my:Model" );
                Assert.That( !this.binaryCache.Exists( "my:Model" ) );
            }

            [Test]
            public async Task ShouldWorkWithObjectsAsKeys()
            {
                var model = new Model{ Title ="title1", ThumbUrl="http://thumb.com/title1.jpg", Description="description1", Price=5.0F };
                var key = new { answer = 42, text = "foo" };
                await this.binaryCache.Set( key, model );

                var result = await this.binaryCache.Get<Model>( key );

                Assert.That( model.Equals( result ) );
                this.binaryCache.Remove( key );
                Assert.That( !this.binaryCache.Exists( key ) );
            }

            [Test]
            public async Task ShouldOverwriteEntries()
            {
                var model1 = new Model{ Title ="title1", ThumbUrl="http://thumb.com/title1.jpg", Description="description1", Price=5.0F };
                var model2 = new Model{ Title ="title2", ThumbUrl="http://thumb.com/title2.jpg", Description="description2", Price=6.0F };
                await this.binaryCache.Set( "myModel", model1 );
                await this.binaryCache.Set( "myModel", model2 );

                var result = await this.binaryCache.Get<Model>( "myModel" );

                Assert.That( !model1.Equals( result ) );
                Assert.That( model2.Equals( result ) );
                this.binaryCache.Remove( "myModel" );
            }



            [Test]
            public async Task ShouldInsertAListOfObjectsToStore()
            {
                var models = new List<Model> {
                    new Model{Title ="title1", ThumbUrl="http://thumb.com/title1.jpg", Description="description1", Price=5.0F},
                    new Model{Title ="title2", ThumbUrl="http://thumb.com/title2.jpg", Description="description2", Price=6.0F},
                    new Model{Title ="title3", ThumbUrl="http://thumb.com/title3.jpg", Description="description3", Price=7.0F},
                };

                await this.binaryCache.Set( "myModels", models );

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
                    await this.binaryCache.Set( "myModel" + i, model );
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
