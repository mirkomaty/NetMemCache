using NUnit.Framework;
using NUnit.Framework.Legacy;
using System.Diagnostics;

namespace NetMemCache.UnitTests
{
    [TestFixture]
    public class CacheTests
    {
		// Perform all tests with DisableDictionary
		// MemCache memCache = new MemCache( "TestStore" ){ DisableDictionary = true };
		MemCache memCache = new MemCache( "TestStore" );

		public CacheTests()
        {
            if (Directory.Exists( memCache.StoreName ))
                Directory.Delete( memCache.StoreName, recursive: true );
        }

        [Test]
        public async Task ShouldInsertAndRetrieveASmallObject()
        {
            var model = new Model{ Title = "Test" };
            await this.memCache.Set( "myModel", model );

            var result = await this.memCache.Get<Model>( "myModel" );

            Assert.That( model.Equals( result ) );
            this.memCache.Remove( "myModel" );
        }

        [Test]
        public async Task ShouldBeAbleToProcessDriveNumbers()
        {
            var model = new Model{ Title = "Test" };
            var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TestStore");
            var memCacheOnDrive = new MemCache( path );
            await memCacheOnDrive.Set( "myModel", model );

            var result = await memCacheOnDrive.Get<Model>( "myModel" );

            Assert.That( model.Equals( result ) );
            memCacheOnDrive.Remove( "myModel" );
        }

        [Test]
        public async Task ShouldInsertAndRetrieveABigObject()
        {
            var model = new Model{ Title ="title1", ThumbUrl="https://nmcache.com/title1.jpg", Description="Lorem ipsum dolor sit amet, consectetuer adipiscing elit. Aenean commodo ligula eget dolor. Aenean massa. Cum sociis natoque penatibus et magnis dis parturient montes, nascetur ridiculus mus. Donec quam felis, ultricies nec, pellentesque eu, pretium quis, sem. Nulla consequat massa quis enim. Donec pede justo, fringilla vel, aliquet nec, vulputate eget, arcu. In enim justo, rhoncus ut, imperdiet a, venenatis vitae, justo. Nullam dictum felis eu pede mollis pretium. Integer tincidunt. Cras dapibus. Vivamus elementum semper nisi. Aenean vulputate eleifend tellus. Aenean leo ligula, porttitor eu, consequat vitae, eleifend ac, enim. Aliquam lorem ante, dapibus in, viverra quis, feugiat a, tellus. Phasellus viverra nulla ut metus varius laoreet. Quisque rutrum. Aenean imperdiet. Etiam ultricies nisi vel augue. Curabitur ullamcorper ultricies nisi. Nam eget dui. Etiam rhoncus. Maecenas tempus, tellus eget condimentum rhoncus, sem quam semper libero, sit amet adipiscing sem neque sed ipsum. Nam quam nunc, blandit vel, luctus pulvinar, hendrerit id, lorem. Maecenas nec odio et ante tincidunt tempus. Donec vitae sapien ut libero venenatis faucibus. Nullam quis ante. Etiam sit amet orci eget eros faucibus tincidunt. Duis leo. Sed fringilla mauris sit amet nibh. Donec sodales sagittis magna. Sed consequat, leo eget bibendum sodales, augue velit cursus nunc,", Price=5.0m };
            await this.memCache.Set( "myModel", model );

            var result = await this.memCache.Get<Model>( "myModel" );

            Assert.That( model.Equals( result ) );
            this.memCache.Remove( "myModel" );
        }


        [Test]
        public async Task ShouldBeAbleToInsertAndRetrieveNull()
        {
            Model model = null;
            await this.memCache.Set( "nullModel", model );

            var result = await this.memCache.Get<Model>( "nullModel" );

            Assert.That( result == null );
            this.memCache.Remove( "nullModel" );
        }

        [Test]
        public async Task ShouldReturnCorrectTypeIfNull()
        {
            await this.memCache.Set<Model>( "myModelTgvNull", null );

            Model result = await this.memCache.Get<Model>( "myModelTgvNull" );

            Assert.That( result == null );
            this.memCache.Remove( "myModelTgvNull" );
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
        public async Task ShouldInsertAndRetrieveWithPath()
        {
            string[] segments = new string[]{"My", "Path"};
            var bcPath = Path.Combine( segments );
            var mc = new MemCache( bcPath );
            var model = new Model{ Title = "Test" };

            await mc.Set( "myModel", model );

            var result = await mc.TryGetValue( "myModel" );

            Assert.That( result.Found, Is.True );
            Assert.That( model.Equals( result.Value ) );
            mc.Remove( "myModel" );
            Directory.Delete( bcPath, true );
        }

        [Test]
        public async Task GetShouldReturnNullIfNotFound()
        {
            var result = await this.memCache.Get<Model>( "notFoundModel" );

            Assert.That( result == null );
        }

        [Test]
        public async Task TryGetValueShouldReturnFalseIfNotFound()
        {
            var result = await this.memCache.TryGetValue( "notFoundModel" );

            Assert.That( !result.Found );
        }

        [Test]
        public async Task TryGetValueShouldRetrieveObject()
        {
            var model = new Model{ Title = "Test" };
            await this.memCache.Set<Model>( "myModelTgv", model );

            var result = await this.memCache.TryGetValue( "myModelTgv" );

            Assert.That( result.Found );
            Assert.That( model.Equals( result.Value ) );
            this.memCache.Remove( "myModelTgv" );
        }

        [Test]
        public async Task ShouldInsertAndGetKeysWithInvalidChars()
        {
            var model = new Model{Title ="title1", ThumbUrl="https://nmcache.com/title1.jpg", Description="description1", Price=5.0m};
            await this.memCache.Set( "my:Model", model );

            var result = await this.memCache.Get<Model>("my:Model");

            Assert.That( model.Equals( result ) );
            this.memCache.Remove( "my:Model" );
            Assert.That( !this.memCache.Exists( "my:Model" ) );
        }

        [Test]
        public async Task ShouldWorkWithObjectsAsKeys()
        {
            var model = new Model{ Title ="title1", ThumbUrl="https://nmcache.com/title1.jpg", Description="description1", Price=5.0m };
            var key = new { answer = 42, text = "foo" };
            await this.memCache.Set( key, model );

            var result = await this.memCache.Get<Model>( key );

            Assert.That( model.Equals( result ) );
            this.memCache.Remove( key );
            Assert.That( !this.memCache.Exists( key ) );
        }

        [Test]
        public async Task ShouldOverwriteEntries()
        {
            var model1 = new Model{ Title ="title1", ThumbUrl="https://nmcache.com/title1.jpg", Description="description1", Price=5.0m };
            var model2 = new Model{ Title ="title2", ThumbUrl="https://nmcache.com/title2.jpg", Description="description2", Price=6.0m };
            await this.memCache.Set( "myModel", model1 );
            await this.memCache.Set( "myModel", model2 );

            var result = await this.memCache.Get<Model>( "myModel" );

            Assert.That( !model1.Equals( result ) );
            Assert.That( model2.Equals( result ) );
            this.memCache.Remove( "myModel" );
        }



        [Test]
        public async Task ShouldInsertAListOfObjectsToStore()
        {
            var models = new List<Model> {
                    new Model{Title ="title1", ThumbUrl="https://nmcache.com/title1.jpg", Description="description1", Price=5.0m},
                    new Model{Title ="title2", ThumbUrl="https://nmcache.com/title2.jpg", Description="description2", Price=6.0m},
                    new Model{Title ="title3", ThumbUrl="https://nmcache.com/title3.jpg", Description="description3", Price=7.0m},
                };

            await this.memCache.Set( "myModels", models );

            var result = await this.memCache.Get<List<Model>>( "myModels" );

            CollectionAssert.AreEqual( models, result );
            this.memCache.Remove( "myModels" );
        }

        [Test]
        public async Task ShouldInsertAndReadAnArrayOfObjects()
        {
            var models = new Model[100];
            for (int i = 0; i < models.Length; i++)
            {
                models[i] = new Model { Title = "title" + ( i + 1 ), ThumbUrl = $"https://nmcache.com/title{i + 1}.jpg", Description = $"description{i}", Price = 5.0m + 1m * i };
            }
            ;

            await this.memCache.Set( "myModels", models );

            var result = await this.memCache.Get<Model[]>( "myModels" );

            Assert.That( result.Length, Is.EqualTo( models.Length ) );

            foreach (var m in result)
                Assert.That( m.Title.StartsWith( "title" ) );

            this.memCache.Remove( "myModels" );
        }

        [Test]
        public async Task ShouldTakeExpiryIntoAccount()
        {
            var model = new Model{ Title = "Test" };
            await this.memCache.Set( "myModel", model, 0 ); // Expires immediately

            // 1st time we get false because of expiration
            var result = await this.memCache.TryGetValue( "myModel" );
            Assert.That( result.Found, Is.False );

            // 2nd time we get false because of object deletion				
            Assert.That( this.memCache.Exists( "myModel" ), Is.False );
        }

        [Test]
        public async Task RemoveExpiredShouldRemoveEntries()
        {
            var model = new Model{ Title = "Test" };
            await this.memCache.Set( "myModel", model, 0 );

            var secondMemCache = new MemCache("TestStore");
            secondMemCache.RemoveExpired();

            var result = await secondMemCache.TryGetValue( "myModel" );

            Assert.That( result.Found, Is.False );
        }

        [Test]
        [Ignore( "Too much files" )]
        public async Task ShouldBeAbleToHandleHeavyLoad()
        {
            var dt = DateTime.Now;
            var count = 10000;
            var trappatoni = "Es gibt im Moment in diese Mannschaft, oh, einige Spieler vergessen ihnen Profi was sie sind. Ich lese nicht sehr viele Zeitungen, aber ich habe gehört viele Situationen. Erstens: wir haben nicht offensiv gespielt. Es gibt keine deutsche Mannschaft spielt offensiv und die Name offensiv wie Bayern. Letzte Spiel hatten wir in Platz drei Spitzen: Elber, Jancka und dann Zickler. Wir müssen nicht vergessen Zickler. Zickler ist eine Spitzen mehr, Mehmet eh mehr Basler. Ist klar diese Wörter, ist möglich verstehen, was ich hab gesagt? Danke. Offensiv, offensiv ist wie machen wir in Platz. Zweitens: ich habe erklärt mit diese zwei Spieler: nach Dortmund brauchen vielleicht Halbzeit Pause. Ich habe auch andere Mannschaften gesehen in Europa nach diese Mittwoch. Ich habe gesehen auch zwei Tage die Training. Ein Trainer ist nicht ein Idiot! Ein Trainer sei sehen was passieren in Platz. In diese Spiel es waren zwei, drei diese Spieler waren schwach wie eine Flasche leer! Haben Sie gesehen Mittwoch, welche Mannschaft hat gespielt Mittwoch? Hat gespielt Mehmet oder gespielt Basler oder hat gespielt Trapattoni? Diese Spieler beklagen mehr als sie spielen! Wissen Sie, warum die Italienmannschaften kaufen nicht diese Spieler? Weil wir haben gesehen viele Male solche Spiel!";
            for (int i = 0; i < count; i++)
            {
                var model = new Model{Title ="title" + i, ThumbUrl=$"https://nmcache.com/title{i}.jpg", Description=$"{trappatoni}{i}", Price=(decimal)i};
                await this.memCache.Set( "myModel" + i, model );
            }

            for (int i = 0; i < count; i++)
            {
                var model = new Model { Title = "title" + i, ThumbUrl = $"https://nmcache.com/title{i}.jpg", Description = $"{trappatoni}{i}", Price = (decimal) i };
                var result = await this.memCache.Get<Model>("myModel" + i);

                Assert.That( model.Equals( result ) );
            }

            var ts = DateTime.Now - dt;
            Debug.WriteLine( "Time Write and Read: " + ts );

            for (int i = 0; i < count; i++)
            {
                this.memCache.Remove( "myModel" + i );
            }

            ts = DateTime.Now - dt;
            Debug.WriteLine( "Time with Delete: " + ts );
        }
    }
}

