using System.Text.Json.Serialization;

namespace SolarWindsTask2.Models
{
    public class RmItem
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = "";

        [JsonPropertyName("url")]
        public string Url { get; set; } = "";
    }
}
