using Newtonsoft.Json;

namespace console_app_rest_client
{
    public class User
    {
        [JsonProperty("login")]
        public string Login { get; set; }

        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("node_id")]
        public string NodeId { get; set; }

        [JsonProperty("location")]
        public string Location { get; set; }
    }
}