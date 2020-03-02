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

            var address = Environment.UserName + "_secrets";

            static Task<string> AddDataAsync(DocumentAddress address) => Task.FromResult(Prompt("Please enter secret!"));

            static void HandleSuccess(string data) => Console.WriteLine(data);

            void HandleError(Exception ex)
            {
                store.DeleteAsync<string>(address).Validate();
                Console.WriteLine(ex.Message);
                Console.WriteLine("Secret deleted!");
            }

            await store
                .GetOrAddAsync<string>(address, AddDataAsync)
                .HandleAsync(HandleSuccess, HandleError);

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
