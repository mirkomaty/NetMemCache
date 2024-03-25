# .NET Object Cache (NOC)
NOC provides a simple and fast key-value store. No database or other technology is needed to store the data. The data is stored in the file system.

This project is based on an idea by Michael Christensen called [BinaryRage](https://github.com/mchidk/BinaryRage). Kudos to him for an incredibly clever idea. After some experimentation with the original project, I made changes step by step so that there isn't much left of the old code.

## Motivation
With Redis there exists a very powerful project for an ObjectCache. But Redis requires a Linux distribution, which is not so easy to implement in an IT landscape with Windows. WSL even in version 2 is not suitable for long running services and [there is an issue with WSL](https://github.com/MicrosoftDocs/WSL/issues/368) not solved by Microsoft.

Moreover, we think there should be a simple, fast .NET object cache available as open source.

## Functionality and Difference to BinaryRage
+ Each class in BinaryRage is static. We reworked the classes to be instantiable to support DI.
+ We implemented a clean asynchronous architecture using async/await.
+ The old code calculated a wait time after save operations, which slowed down the system unnecessarily. By using async/await, the store was able to be significantly accelerated.
+ We made all core components replacable with DI, e.g. IStorage, IFolderStructure, IObjectSerializer
+ The original implementation serializes the objects in-memory. We write the objects directly into the output stream.
+ We removed the class SimpleObject because, in our opinion, it has no utility value.
+ We implemented an optional expiration date so that cache entries can be removed after a certain period of time.
+ We changed the algorithm that calculates the directory structure so that all objects reside in a maximum of 65536 directories. This behavior can be changed.
+ The original algorithm uses the BinaryFormatter. We changed this in favour of Newtonsoft.Json. The algorithm can now be injected via DependencyInjection.
+ The old algorithm serialized all input objects. We don't serialize strings. These will be compressed directly if they are long enough. It is also saved whether deserialization is necessary.
+ The original algorithm compresses all objects. We only compress objects of a certain size and save whether compression has taken place.
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

Insert-syntax (same for create and update)

	var objectCache = new ObjectCache("productCache");
	objectCache.Insert("mykey", @"C:\testpath");
	
	// removal of an object after a certain time span
	objectCache.Insert("mykey", @"C:\testpath", timeSpan);

... or with a list

	objectCache.Insert("mykey", myProduct);

Get the saved data

	var myProduct = objectCache.Get<Product>("mykey");
	
... or with a list

	var listOfProducts = objectCache.Get<List<Product>>("mykey");

Query objects directly with LINQ

	var bestsellers = objectCache.Get<List<Category>>("bestsellers").Where(p => !string.IsNullOrEmpty(p.Name));

Note: GetJSON and WaitForCompletion have been removed. Key API has been removed.

# FAQ
## Is it really fast?
On my development machine, which is not the newest technology I measured about 0.6 ms for each read / write pair.

All writes are performed asynchronously. Reads are instantly available (also if writes are not completed) because of the in-memory-cache.

