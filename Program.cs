using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using Refit;
using CurlThin;
using CurlThin.Enums;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using CurlThin.Helpers;

namespace console_app_rest_client
{
    class Program
    {

        private static readonly HttpClient client = new HttpClient();


        static async Task Main(string[] args)
        {
            var serviceCollection = new ServiceCollection();
            // Config origin http client.
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
            // var github = services.GetRequiredService<GithubClient>();
            // ProcessRepositories(github).GetAwaiter();

            // Rest sharp client using sync way
            // UsingRestSharpClient();

            // Rest sharp client using async way
            // UsingRestSharpClientAsync();

            // Refit client
            // await UsingRefitClient();

            // CurlThin client
            UsingCurlThinGet();
            // UsingCurlThinPost();
            // UsingCurlThinMulti();


            Console.ReadLine();
        }

        private static void UsingCurlThinGet()
        {
            var global = CurlNative.Init();

            // curl_easy_init() to create easy handle.
            var easy = CurlNative.Easy.Init();
            try
            {
                CurlNative.Easy.SetOpt(easy, CURLoption.URL, "https://api.github.com/users/octocat");
                // CurlNative.Easy.SetOpt(easy, CURLoption.URL, "http://httpbin.org/ip");

                var stream = new MemoryStream();
                var curlSlist = new CurlSlist();
                curlSlist.Append("User-Agent: Awesome Octocat App");
                CurlNative.Easy.SetOpt(easy, CURLoption.HTTPHEADER, curlSlist.Handle);

                CurlNative.Easy.SetOpt(easy, CURLoption.WRITEFUNCTION, (data, size, nmemb, user) =>
                {
                    var length = (int)size * (int)nmemb;
                    var buffer = new byte[length];
                    Marshal.Copy(data, buffer, 0, length);
                    stream.Write(buffer, 0, length);
                    return (UIntPtr)length;
                });

                var result = CurlNative.Easy.Perform(easy);

                Console.WriteLine($"Result code: {result}.");
                Console.WriteLine();
                Console.WriteLine("Response body:");
                Console.WriteLine(Encoding.UTF8.GetString(stream.ToArray()));
            }
            finally
            {
                easy.Dispose();

                if (global == CURLcode.OK)
                {
                    CurlNative.Cleanup();
                }
            }
        }

        private static void UsingCurlThinMulti()
        {
            var hyper = new HyperSample();
            hyper.Run();
            Console.WriteLine("Finished! Press ENTER to exit...");
        }

        private static void UsingCurlThinPost()
        {
            // curl_global_init() with default flags.
            var global = CurlNative.Init();

            // curl_easy_init() to create easy handle.
            var easy = CurlNative.Easy.Init();
            try
            {
                var postData = "fieldname1=fieldvalue1&fieldname2=fieldvalue2";

                CurlNative.Easy.SetOpt(easy, CURLoption.URL, "http://httpbin.org/post");

                // This one has to be called before setting COPYPOSTFIELDS.
                CurlNative.Easy.SetOpt(easy, CURLoption.POSTFIELDSIZE, Encoding.ASCII.GetByteCount(postData));
                CurlNative.Easy.SetOpt(easy, CURLoption.COPYPOSTFIELDS, postData);

                var dataCopier = new DataCallbackCopier();
                CurlNative.Easy.SetOpt(easy, CURLoption.WRITEFUNCTION, dataCopier.DataHandler);

                var result = CurlNative.Easy.Perform(easy);

                Console.WriteLine($"Result code: {result}.");
                Console.WriteLine();
                Console.WriteLine("Response body:");
                Console.WriteLine(Encoding.UTF8.GetString(dataCopier.Stream.ToArray()));
            }
            finally
            {
                easy.Dispose();

                if (global == CURLcode.OK)
                {
                    CurlNative.Cleanup();
                }
            }
        }

        private static async Task UsingRefitClient()
        {
            var gitHubApi = RestService.For<IGitHubApi>("https://api.github.com/");
            var user = await gitHubApi.GetUser("octocat");
            // var repositories = await gitHubApi.GetDotnetRepositories();
            // PrintRepositories(repositories);
        }

        private static void UsingRestSharpClient()
        {
            var client = new RestClient("https://api.github.com/orgs/dotnet/repos");
            var request = new RestRequest(Method.GET);
            request.AddHeader("User-Agent", ".NET Foundation Repository Reporter");
            var response = client.Execute(request);
            var data = response.Content; // raw content as string
            var repositories = JsonConvert.DeserializeObject<IEnumerable<Repo>>(data);
            PrintRepositories(repositories);
        }

        private static void UsingRestSharpClientAsync()
        {
            var client = new RestClient("https://api.github.com/orgs/dotnet/repos");
            var request = new RestRequest(Method.GET);
            request.AddHeader("User-Agent", ".NET Foundation Repository Reporter");
            client.ExecuteAsync(request, response =>
            {
                var data = response.Content; // raw content as string
                var repositories = JsonConvert.DeserializeObject<IEnumerable<Repo>>(data);
                PrintRepositories(repositories);
            });
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

        [Headers("User-Agent: Awesome Octocat App")]
        public interface IGitHubApi
        {
            [Get("/orgs/dotnet/repos")]
            Task<List<Repo>> GetDotnetRepositories();

            [Get("/users/{user}")]
            Task<User> GetUser(string user);
        }
    }

}
