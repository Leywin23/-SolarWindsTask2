using System.Net;
using System.Text.Json;
using SolarWindsTask2.Dtos;
using SolarWindsTask2.Interfaces;
using SolarWindsTask2.Models;

namespace SolarWindsTask2.Clients;

public class RickAndMortyClient : IRickAndMortyClient
{
    private readonly HttpClient _client;
    private readonly JsonSerializerOptions _json;

    public RickAndMortyClient(HttpClient client)
    {
        _client = client;
        _json = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
    }

    public async Task<(List<RmItem> Characters, List<RmItem> Locations, List<RmItem> Episodes)> SearchAllAsync(string term)
    {
        var t = Uri.EscapeDataString(term);

        var chTask = GetPageOrEmptyAsync<RmItem>($"character?name={t}");
        var loTask = GetPageOrEmptyAsync<RmItem>($"location?name={t}");
        var epTask = GetPageOrEmptyAsync<RmItem>($"episode?name={t}");

        await Task.WhenAll(chTask, loTask, epTask);

        return (chTask.Result, loTask.Result, epTask.Result);
    }

    public async Task<List<EpisodeCharacters>> GetAllEpisodesAsync()
    {
        var first = await GetAsync<RmPage<EpisodeItem>>("episode?page=1");
        var pages = first.Info?.Pages ?? 1;

        var result = new List<EpisodeCharacters>();
        result.AddRange(MapEpisodeIds(first));

        for (int p = 2; p <= pages; p++)
        {
            var page = await GetAsync<RmPage<EpisodeItem>>($"episode?page={p}");
            result.AddRange(MapEpisodeIds(page));
        }

        return result;
    }

    public async Task<Dictionary<int, string>> GetCharacterNamesAsync(List<int> ids)
    {
        var map = new Dictionary<int, string>();
        ids = ids.Distinct().OrderBy(x => x).ToList();

        const int batchSize = 50;
        for (int i = 0; i < ids.Count; i += batchSize)
        {
            var batch = ids.Skip(i).Take(batchSize).ToList();
            var path = $"character/{string.Join(",", batch)}";

            // API: 1 id => obiekt, wiele => tablica
            if (batch.Count == 1)
            {
                var one = await GetAsync<CharacterItem>(path);
                map[one.Id] = one.Name ?? $"#{one.Id}";
            }
            else
            {
                var many = await GetAsync<List<CharacterItem>>(path);
                foreach (var ch in many)
                    map[ch.Id] = ch.Name ?? $"#{ch.Id}";
            }
        }

        return map;
    }

    private async Task<List<T>> GetPageOrEmptyAsync<T>(string path)
    {
        var resp = await _client.GetAsync(path);

        if (resp.StatusCode == HttpStatusCode.NotFound)
            return new List<T>();

        resp.EnsureSuccessStatusCode();

        var json = await resp.Content.ReadAsStringAsync();
        var page = JsonSerializer.Deserialize<RmPage<T>>(json, _json);
        return page?.Results ?? new List<T>();
    }

    private async Task<T> GetAsync<T>(string path)
    {
        var resp = await _client.GetAsync(path);
        resp.EnsureSuccessStatusCode();

        var json = await resp.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<T>(json, _json)!;
    }

    private static IEnumerable<EpisodeCharacters> MapEpisodeIds(RmPage<EpisodeItem> page)
    {
        foreach (var ep in page.Results ?? new List<EpisodeItem>())
        {
            var ids = new List<int>();

            foreach (var url in ep.Characters ?? new List<string>())
            {
                var idx = url.LastIndexOf('/');
                if (idx < 0) continue;
                if (int.TryParse(url[(idx + 1)..], out var id))
                    ids.Add(id);
            }

            yield return new EpisodeCharacters(ids);
        }
    }
}