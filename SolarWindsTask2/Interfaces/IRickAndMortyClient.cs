using SolarWindsTask2.Models;

namespace SolarWindsTask2.Interfaces
{
    public interface IRickAndMortyClient
    {
        Task<(List<RmItem> Characters, List<RmItem> Locations, List<RmItem> Episodes)> SearchAllAsync(string term);
        Task<List<EpisodeCharacters>> GetAllEpisodesAsync();
        Task<Dictionary<int, string>> GetCharacterNamesAsync(List<int> ids);
    }
}
