# DocumentStores

[![](https://github.com/JanDonnermayer/DocumentStores/workflows/UnitTests/badge.svg)](
https://github.com/JanDonnermayer/DocumentStores/actions)

[![](https://img.shields.io/badge/nuget-v0.1.2-blue.svg)](
https://www.nuget.org/packages/DocumentStores/)

A file-based key-value-store, using the popular Newtonsoft.Json serialization library.  
Features include exception-handling via result-types, and semaphores for thread-safety.

## Adding the Package

```powershell
dotnet add package DocumentStores
```

## Using the Store

```csharp
// Define your model
class Person { public string name; public int age; }

// Create new json-file-store, using default directory: $(appData)/$(appName)/
var store = new JsonFileDocumentStore();

// Put some data
await store.PutDocumentAsync("person1", new Person { name = "Jan", age = 24 });

// Read the data.
// The store does not throw Exceptions (but ArgumentExceptions). All methods return results.
var result = await store.GetDocumentAsync<Person>("person1");

// Access the data or throw exception in case of errors.
Person person = result.Validate();
```

You can process the result in more sophisticated ways.

Using the _Try_-method,

```csharp
if (result.Try(out Person? person, out Exception? ex))
    HandleData(person!)
else
    HandleError(ex!);
```

... or the _Handle_-method,

```csharp
result.Handle(HandleData, HandleError);
```

Similar methods exist for result-tasks.

```csharp
Person person = await store.GetDocumentAsync(...).ValidateAsync();
```

```csharp
await store.GetDocumentAsync(...).HandleAsync(HandleData, HandleError);
```

## Optimized Usage

You can define type-bound partitions within a store, so-called _Topics_.

```csharp
var maintainerTopic = store.ToTopic<Person>("maintainers");

await maintainerTopic.PutAsync("jan", new Person { name = "Jan", age = 24 }).ValidateAsync();
await maintainerTopic.PutAsync("elisa", new Person { name = "Elisa", age = 22 }).ValidateAsync();

var maintainers = await maintainerTopic.GetAllAsync();
```

If you repeatedly access a single document, create a _Channel_,
a syntactic shortcut for omitting the key.

```csharp
var ownerChannel = maintainerTopic.ToChannel("jan");

Person owner = await ownerChannel.GetAsync().ValidateAsync();
owner.age += 1;
await ownerChannel.PutAsync(owner);
```

For caching scenarios, the _.GetOrAdd()_ method comes in handy.

```csharp
var data = await dataTopic.GetOrAddAsync("id", id => api.GetAsync(id));
```

The _.AddOrUpdate()_ is useful when storing collections.

```csharp
await settingsChannel.AddOrUpdateAsync(
    initialData: ImmutableList.Create("val"),
    updateData: list => list.Add("val")
);
```

## Advanced Configuration

To chose a different location for the store, initialize it like so:

```csharp
var store = new JsonFileDocumentStore(rootDirectory: "C:/Store");
```

Since v0.1.3-preview3, encryption is supported.
The provided password serves as key for Rijndael algorithm.

```csharp
var store = new JsonFileDocumentStore(password: "myPassword");
```

More can be configured by using the options:

```csharp
var options = JsonFileDocumentStoreOptions
    .Default
    .WithRootDirectory("C:/Temp")
    .WithEncryptionOptions(
        EncryptionOptions.Aes
            .WithKey(new byte[] { 1, 2, 3, 4 })
            .WithIV(new byte[] { 1, 2, 3, 4 })
        )
    );

var store = new JsonFileDocumentStore(options);
```
