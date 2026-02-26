using System.Text.Json.Serialization;

namespace SolarWindsTask2.Dtos
{
    public class TopPairDto
    {
        [JsonPropertyName("character1")]
        public CharacterRefDto Character1 { get; set; } = new();

        [JsonPropertyName("character2")]
        public CharacterRefDto Character2 { get; set; } = new();

        [JsonPropertyName("episodes")]
        public int Episodes { get; set; }
    }
}
