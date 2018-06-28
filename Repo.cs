using System;
using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace console_app_rest_client
{
    public class Repo
    {
        public string name { get; set; }

        [JsonProperty("html_url")]
        public Uri GitHubHomeUrl { get; set; }

        [JsonProperty("full_name")]
        public string FullName { get; set; }

    }
}