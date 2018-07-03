﻿using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

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
                    c.BaseAddress = new Uri("https://api.github.com/");
                    c.DefaultRequestHeaders.Add("Accept", "application/vnd.github.v3+json");
                    c.DefaultRequestHeaders.Add("User-Agent", ".NET Foundation Repository Reporter");
                })
                .AddTypedClient<GithubClient>();

            var services = serviceCollection.BuildServiceProvider();

            // Simple http client.
            // ProcessRepositories().GetAwaiter();

            // Using http client factory to create client.
            // var githubClient = services
            //     .GetRequiredService<IHttpClientFactory>()
            //     .CreateClient("github");

            // ProcessRepositories(githubClient).GetAwaiter();

            // Using typed http client that retrieved from DI container.
            var github = services.GetRequiredService<GithubClient>();
            ProcessRepositories(github).GetAwaiter();

            Console.ReadLine();
        }

        private static async Task ProcessRepositories(GithubClient github)
        {
            var response = await github.GetData();
            // var data = await response.Content.ReadAsAsync<JObject>();
            var data = await response.Content.ReadAsStringAsync();

            var repositories = JsonConvert.DeserializeObject<IEnumerable<Repo>>(data);
            PrintRepositories(repositories);
        }

        private static async Task ProcessRepositories(HttpClient client)
        {
            var stringTask = client.GetStringAsync($"{client.BaseAddress}/orgs/dotnet/repos");
            var repositories = JsonConvert.DeserializeObject<IEnumerable<Repo>>(await stringTask);
            PrintRepositories(repositories);
        }

        private static async Task ProcessRepositories()
        {
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/vnd.github.v3+json"));
            client.DefaultRequestHeaders.Add("User-Agent", ".NET Foundation Repository Reporter");

            var stringTask = client.GetStringAsync("https://api.github.com/orgs/dotnet/repos");

            var repositories = JsonConvert.DeserializeObject<IEnumerable<Repo>>(await stringTask);
            PrintRepositories(repositories);
        }

        private static void PrintRepositories(IEnumerable<Repo> repositories)
        {
            foreach (var repo in repositories)
            {
                Console.WriteLine($"Repo name:{repo.name} .Full Name:{repo.FullName}. URI:{repo.GitHubHomeUrl}");
            }
        }

        private class GithubClient
        {
            public GithubClient(HttpClient httpClient)
            {
                HttpClient = httpClient;
            }

            public HttpClient HttpClient { get; }

            // Gets the list of services on github.
            public async Task<HttpResponseMessage> GetData()
            {
                // var request = new HttpRequestMessage(HttpMethod.Get, "/");
                var request = new HttpRequestMessage(HttpMethod.Get, "/orgs/dotnet/repos");


                var response = await HttpClient.SendAsync(request).ConfigureAwait(false);
                response.EnsureSuccessStatusCode();

                return response;
            }
        }
    }

}
