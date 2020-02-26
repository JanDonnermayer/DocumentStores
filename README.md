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
// The store does not throw IO-Exceptions. All methods return results.
var result = await store.GetDocumentAsync<Person>("person1");

// Query the result, re-throwing any occurred exceptions
var person = result.PassOrThrow();
```

### You can process the result in more sophisticated ways

Using the .Try method

```csharp
if (result.Try(out var person, out var ex))
    Process(person)
else
    HandleError(ex);
```

Or with C# 8.0 Recursive Patterns

```csharp
_ = result switch
{
    (Person person, _) => Process(person),
    (_, Exception ex) => HandleError(ex)
}
```

If you are a fan of null-pointers, feel free to direct cast or use .Data property

```csharp
Person? person = result;
person = result.Data;
```

## Optimized Usage

You can define type-bound partitions within a store, so-called _Topics_.

```csharp
var maintainerTopic = store.ToTopic<Person>("maintainers");

await maintainerTopic.PutAsync("jan", new Person { name = "Jan", age = 24 });
await maintainerTopic.PutAsync("elisa", new Person { name = "Elisa", age = 22 });

var maintainers = await maintainerTopic.GetAllAsync();
```

If you repeatedly access a single document, create a _Channel_.

```csharp
var ownerChannel = maintainerTopic.ToChannel("jan");

Person owner = await ownerChannel.GetAsync();
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

v0.1.3-preview3 supports encryption.
The specified password serves as key for AES.
Note that either all data is encrypted or none.
More fine-grained configuration will be added soon.

```csharp
var store = new JsonFileDocumentStore(password: "myPassword");
```

More can be configured by using the options:

```csharp
var options = JsonFileDocumentStoreOptions
    .Default
    .WithRootDirectory("C:/Temp")
    .WithEncryptionOptions(
        EncryptionOptions.Aes(
            key: new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 },
            iV: new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 }
        )
    );

var store = new JsonFileDocumentStore(options);
```
