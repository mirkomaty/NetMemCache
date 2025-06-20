# NetMemCache
NetMemCache provides a simple and fast key-value store. No database or other technology is needed to store the data. The data is stored in the file system.

This project is based on a project by Michael Christensen called [BinaryRage](https://github.com/mchidk/BinaryRage). Kudos to him for an incredibly clever idea. After some experimentation with the original project, I made changes step by step so that there isn't much left of the old code.

## Motivation
With Redis there exists a very powerful project for an ObjectCache. But Redis requires a Linux distribution, which is not so easy to implement in an IT landscape with Windows. WSL even in version 2 is not suitable for long running services and [there is an issue with WSL](https://github.com/MicrosoftDocs/WSL/issues/368) not solved by Microsoft.

Moreover, we think there should be a simple, fast .NET object cache available as open source.

## Functionality and Difference to BinaryRage
+ We reworked the classes to be instantiable to support DI, unlike BinaryRage, where all methods are static.
+ We implemented a clean asynchronous architecture using async/await.
+ The old code calculated a wait time after save operations, which slowed down the system unnecessarily. By using async/await, the store was able to be significantly accelerated.
+ We made all core components replacable with DI, e.g. IStorage, IFolderStructure, IObjectSerializer
+ The original implementation serializes the objects in-memory. We write the objects directly into the output stream.
+ We removed the class SimpleObject because, in our opinion, it has no utility value.
+ We implemented an optional expiration date so that cache entries can be removed after a certain period of time.
+ We changed the algorithm that calculates the directory structure so that all objects reside in a maximum of 65536 directories. This behavior can be changed.
+ The original algorithm uses the BinaryFormatter. We changed this in favour of Newtonsoft.Json. The algorithm can now be injected via DependencyInjection.
+ The old algorithm serialized all input objects. We don't serialize strings. If they are long enough, they will be compressed. If an object was serialized an deserialization is necessary during read, this will be stored in the header data.
+ The original algorithm compresses all objects. We only compress objects of a certain size and store the information whether compression has taken place.
+ We added a WebAPI project to use the ObjectCache as a service.
+ Future feature: Saving objects from a stream, e.g. after upload of data to a web application

## Show me the code
The samples are intentionally derived from the BinaryRage samples to allow comparism.
Simple class - no serializable attribute needed.

	public class Product
	{
		public string Title { get; set; }
		public string ThumbUrl { get; set; }
		public string Description { get; set; }
		public float Price { get; set; }
	}

	var myProduct = new Product(){...};

Insert-syntax (same for create and update)

	var memCache = new MemCache("productCache");
	memCache.Set("mykey", myProduct);
	
	// removal of an object after a certain time span
	memCache.Set("mykey", myProduct, timeSpan);

... or with a list

	var myProducts = new List<Product>();
	memCache.Insert("mykey", myProducts);

Get the saved data

	var myProduct = memCache.Get<Product>("mykey");
	
... or with a list

	var listOfProducts = memCache.Get<List<Product>>("mykey");

 Check, if data is available:

```
var result = await memCache.TryGetValue( key );
if (result.Found)
    return Content( (string) result.Value );
else
    return NotFound();
```

Remove Entries:

```
memcache.Remove(key);
```

Removal for housekeeping of expired objects:

```
memcache.RemoveExpired();
```

Call this method regularly.

# FAQ
## Is it fast?
On my development machine, which is not the newest technology I measured about 0.36 ms for each write / read pair. Getting the values costs ~0.0025 ms of the total time. 

If we have to read all values from disc (restarting the test application with known keys), reading costs about 0,13 ms per entry.

Note that these costs apply to small objects (e.g. guids) that do not need to be compressed. Compressing larger objects takes additional time.

The total costs of a write / read cycle with compressed data are 0.59 ms. Reading these values from the file system costs 0,13 ms per entry.

Deleting all objects with Remove() costs about 0.17 ms per entry. Note, that deleting doesn't delete the folders. That would be very inefficient.

All writes are performed asynchronously. Because of the in-memory-cache reads are available immediately, even if writing is not yet complete.
