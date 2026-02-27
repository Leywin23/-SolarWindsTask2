using SolarWindsTask2.Dtos;

namespace SolarWindsTask2.Interfaces
{
    public interface ITopPairsService
    {
        Task<List<TopPairDto>> GetTopPairsAsync(int? min, int? max, int? limit);
    }
}
