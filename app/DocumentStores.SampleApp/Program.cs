using System;
using System.Threading.Tasks;
using static System.Console;

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

            static void HandleSuccess(string data) => WriteLines("The secret is:", data);

            void HandleError(Exception ex)
            {
                store.DeleteAsync<string>(address).Validate();
                WriteLines(ex.Message, "Secret deleted!");
            }

            await store
                .GetOrAddAsync<string>(address, AddDataAsync)
                .HandleAsync(HandleSuccess, HandleError);

            PromptKey("Press any key");
        }


        private static void WriteLines(string firstLines, params string[] lines)
        {
            WriteLine(firstLines);
            foreach (var line in lines) WriteLine(line);
        }


        private static string Prompt(string message)
        {
            WriteLine(message);
            return ReadLine();
        }

        private static void PromptKey(string message)
        {
            WriteLine(message);
            ReadKey();
        }
    }
}
