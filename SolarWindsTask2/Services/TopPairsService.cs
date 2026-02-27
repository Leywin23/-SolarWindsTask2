using SolarWindsTask2.Clients;
using SolarWindsTask2.Dtos;
using SolarWindsTask2.Interfaces;
using SolarWindsTask2.Models;

namespace SolarWindsTask2.Services;

public class TopPairsService : ITopPairsService
{
    private readonly IRickAndMortyClient _rm;

    public TopPairsService(IRickAndMortyClient rm) => _rm = rm;

    public async Task<List<TopPairDto>> GetTopPairsAsync(int? min, int? max, int? limit)
    {
        if (min.HasValue && min.Value < 0) throw new ArgumentException("min must be >= 0");
        if (max.HasValue && min.HasValue && max.Value < min.Value) throw new ArgumentException("max must be >= min");

        int take = limit ?? 20;
        if (take < 0) take = 0;

        var episodes = await _rm.GetAllEpisodesAsync();

        var pairCounts = new Dictionary<(int A, int B), int>();

        foreach (var ep in episodes)
        {
            var ids = ep.CharacterIds.Distinct().OrderBy(x => x).ToList();

            for (int i = 0; i < ids.Count; i++)
                for (int j = i + 1; j < ids.Count; j++)
                {
                    var key = (ids[i], ids[j]);
                    pairCounts[key] = pairCounts.TryGetValue(key, out var c) ? c + 1 : 1;
                }
        }

        var top = pairCounts
            .Where(kvp => (!min.HasValue || kvp.Value >= min.Value) && (!max.HasValue || kvp.Value <= max.Value))
            .OrderByDescending(kvp => kvp.Value)
            .ThenBy(kvp => kvp.Key.A)
            .ThenBy(kvp => kvp.Key.B)
            .Take(take)
            .ToList();

        if (!top.Any())
            return new List<TopPairDto>();

        var idsNeeded = top.SelectMany(x => new[] { x.Key.A, x.Key.B }).Distinct().ToList();
        var idToName = await _rm.GetCharacterNamesAsync(idsNeeded);

        return top.Select(kvp =>
        {
            var a = kvp.Key.A;
            var b = kvp.Key.B;

            return new TopPairDto
            {
                Character1 = new CharacterRefDto
                {
                    Name = idToName.TryGetValue(a, out var an) ? an : $"#{a}",
                    Url = $"https://rickandmortyapi.com/api/character/{a}"
                },
                Character2 = new CharacterRefDto
                {
                    Name = idToName.TryGetValue(b, out var bn) ? bn : $"#{b}",
                    Url = $"https://rickandmortyapi.com/api/character/{b}"
                },
                Episodes = kvp.Value
            };
        }).ToList();
    }
}