using System;
using System.Threading.Tasks;

namespace DocumentStores.SampleApp
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var store = new JsonFileDocumentStore(
                password: Prompt("Please enter password!")
            );

            await store
                .GetOrAddAsync(
                    address: Environment.UserName + "_secrets",
                    addDataAsync: _ => Task.FromResult(Prompt("Please enter secret!"))
                )
                .HandleAsync(
                    dataHandler: Console.WriteLine,
                    errorHandler: ex => Console.WriteLine(ex.Message)
                )
                .ConfigureAwait(false);

            PromptKey("Press any key");
        }

        private static string Prompt(string message)
        {
            Console.WriteLine(message);
            return Console.ReadLine();
        }

        private static void PromptKey(string message)
        {
            Console.WriteLine(message);
            Console.ReadKey();
        }
    }
}
