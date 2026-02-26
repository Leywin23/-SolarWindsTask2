using SolarWindsTask2.Dtos;

namespace SolarWindsTask2.Interfaces
{
    public interface ISearchService
    {
        Task<List<ResultDto>> SearchAsync(string term, int? limit);
    }
}
