using System.Text.Json.Serialization;

namespace SolarWindsTask2.Models
{
    public class CharacterItem
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("name")]
        public string? Name { get; set; }
    }
}
