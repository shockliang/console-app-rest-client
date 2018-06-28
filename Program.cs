using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace console_app_rest_client
{
    class Program
    {
        private static readonly HttpClient client = new HttpClient();
        static void Main(string[] args)
        {
            ProcessRepositories().Wait();
            Console.ReadLine();
        }

        private static async Task ProcessRepositories()
        {
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/vnd.github.v3+json"));
            client.DefaultRequestHeaders.Add("User-Agent", ".NET Foundation Repository Reporter");

            var stringTask = client.GetStringAsync("https://api.github.com/orgs/dotnet/repos");

            var repositories = JsonConvert.DeserializeObject<IEnumerable<Repo>>(await stringTask);
            foreach (var repo in repositories)
            {
                Console.WriteLine($"Repo name:{repo.name} .Full Name:{repo.FullName}. URI:{repo.GitHubHomeUrl}");
            }
        }
    }

}
