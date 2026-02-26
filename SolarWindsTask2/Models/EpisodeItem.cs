using System.Text.Json.Serialization;

namespace SolarWindsTask2.Models
{
    public class EpisodeItem
    {
        [JsonPropertyName("characters")]
        public List<string>? Characters { get; set; }
    }
}
