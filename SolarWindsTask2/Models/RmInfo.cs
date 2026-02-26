using System.Text.Json.Serialization;

namespace SolarWindsTask2.Models
{
    public class RmInfo
    {
        [JsonPropertyName("pages")]
        public int Pages { get; set; }
    }
}
