# DocumentStores

[![](https://github.com/JanDonnermayer/DocumentStores/workflows/UnitTests/badge.svg)](
https://github.com/JanDonnermayer/DocumentStores/actions)

[![](https://img.shields.io/badge/nuget-v0.0.8-blue.svg)](
https://www.nuget.org/packages/DocumentStores/)

A file-based key-value-store, using the popular Newtonsoft.Json serialization library.  
Features include exception-handling via result-types, and semaphores for thread-safety.

## Usage

```csharp

    class Person { public string name; public int age; }

    var service = new JsonFileDocumentStore(DIRECTORY)
        .AsObservableDocumentStore<Person>();

    const KEY = "maintainer";

    await service.AddOrUpdateDocumentAsync(
        key: KEY,
        initialData: new Person() { name = "Jan", age = 24 },
        updateData: p => new Person() { name = p.name, age = p.age + 1 });

    await service.DeleteDocumentAsync(KEY);
```

## Dotnet CLI

```powershell
dotnet add package DocumentStores 
```