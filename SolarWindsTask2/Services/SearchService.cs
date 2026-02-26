using SolarWindsTask2.Clients;
using SolarWindsTask2.Dtos;
using SolarWindsTask2.Interfaces;

namespace SolarWindsTask2.Services;

public class SearchService : ISearchService
{
    private readonly IRickAndMortyClient _rm;

    public SearchService(IRickAndMortyClient rm) => _rm = rm;

    public async Task<List<ResultDto>> SearchAsync(string term, int? limit)
    {
        term = term?.Trim() ?? "";
        if (string.IsNullOrWhiteSpace(term))
            throw new ArgumentException("term is required");

        var (characters, locations, episodes) = await _rm.SearchAllAsync(term);

        var all = new List<ResultDto>();
        all.AddRange(characters.Select(x => new ResultDto { Name = x.Name, Type = "character", Url = x.Url }));
        all.AddRange(locations.Select(x => new ResultDto { Name = x.Name, Type = "location", Url = x.Url }));
        all.AddRange(episodes.Select(x => new ResultDto { Name = x.Name, Type = "episode", Url = x.Url }));

        if (limit.HasValue && limit.Value >= 0)
            all = all.Take(limit.Value).ToList();

        return all;
    }
}