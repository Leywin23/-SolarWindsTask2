using Microsoft.AspNetCore.Mvc;
using SolarWindsTask2.Dtos;
using SolarWindsTask2.Interfaces;

namespace SolarWindsTask2.Controllers;

[ApiController]
[Route("api")]
public class TasksController : ControllerBase
{
    private readonly ISearchService _search;
    private readonly ITopPairsService _topPairs;

    public TasksController(ISearchService search, ITopPairsService topPairs)
    {
        _search = search;
        _topPairs = topPairs;
    }

    [HttpGet("search")]
    public async Task<ActionResult<List<ResultDto>>> Search([FromQuery] string term, [FromQuery] int? limit)
        => Ok(await _search.SearchAsync(term, limit));

    [HttpGet("top-pairs")]
    public async Task<ActionResult<List<TopPairDto>>> TopPairs([FromQuery] int? min, [FromQuery] int? max, [FromQuery] int? limit)
        => Ok(await _topPairs.GetTopPairsAsync(min, max, limit));
}