using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace console_app_rest_client
{
    class Program
    {

        private static readonly HttpClient client = new HttpClient();


        static void Main(string[] args)
        {
            var serviceCollection = new ServiceCollection();
            serviceCollection
                .AddLogging(builder =>
                {
                    builder.AddFilter((category, level) => true); // Spam the world with logs.

                    // Add console logger so we can see all the logging produced by the client by default.
                    builder.AddConsole(c => c.IncludeScopes = true);
                })
                .AddHttpClient("github", c =>
                {
                    c.BaseAddress = new Uri("https://api.github.com/orgs/dotnet/repos");
                    c.DefaultRequestHeaders.Add("Accept", "application/vnd.github.v3+json");
                    c.DefaultRequestHeaders.Add("User-Agent", ".NET Foundation Repository Reporter");
                });

            var services = serviceCollection.BuildServiceProvider();

            var githubClient = services
                .GetRequiredService<IHttpClientFactory>()
                .CreateClient("github");

            ProcessRepositories(githubClient).GetAwaiter();

            Console.ReadLine();
        }

        private static async Task ProcessRepositories(HttpClient client)
        {
            var stringTask = client.GetStringAsync(client.BaseAddress);
            var repositories = JsonConvert.DeserializeObject<IEnumerable<Repo>>(await stringTask);
            foreach (var repo in repositories)
            {
                Console.WriteLine($"Repo name:{repo.name} .Full Name:{repo.FullName}. URI:{repo.GitHubHomeUrl}");
            }
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
