using SolarWindsTask2.Dtos;
using System.Text.Json.Serialization;

namespace SolarWindsTask2.Models
{
    public class RmPage<T>
    {
        [JsonPropertyName("info")]
        public RmInfo? Info { get; set; }

        [JsonPropertyName("results")]
        public List<T>? Results { get; set; }
    }

}
