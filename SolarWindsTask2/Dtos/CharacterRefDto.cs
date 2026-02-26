using System.Text.Json.Serialization;

namespace SolarWindsTask2.Dtos
{
    public class CharacterRefDto
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = "";

        [JsonPropertyName("url")]
        public string Url { get; set; } = "";
    }
}
