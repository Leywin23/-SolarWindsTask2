using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Moq;
using SolarWindsTask2.Interfaces;
using SolarWindsTask2.Models;
using SolarWindsTask2.Services;
using Xunit;

namespace SolarWindsTask2_UnitTests.Services
{
    public class TopPairsServiceTests
    {
        [Fact]
        public async Task GetTopPairsAsync_ReturnsPairsOrderedAndFiltered()
        {
            var mock = new Mock<IRickAndMortyClient>();
            var episodes = new List<EpisodeCharacters>
            {
                new EpisodeCharacters(new List<int>{1,2,3}),
                new EpisodeCharacters(new List<int>{1,2}),
                new EpisodeCharacters(new List<int>{2,3})
            };
            mock.Setup(m => m.GetAllEpisodesAsync()).ReturnsAsync(episodes);
            mock.Setup(m => m.GetCharacterNamesAsync(It.IsAny<List<int>>()))
                .ReturnsAsync((List<int> ids) => ids.ToDictionary(i => i, i => $"Name{i}"));

            var svc = new TopPairsService(mock.Object);
            var result = await svc.GetTopPairsAsync(null, null, null);

            Assert.NotEmpty(result);
            Assert.Equal(2, result.First().Episodes);
        }

        [Fact]
        public async Task GetTopPairsAsync_AppliesMinMaxFilter()
        {
            var mock = new Mock<IRickAndMortyClient>();
            var episodes = new List<EpisodeCharacters>
            {
                new EpisodeCharacters(new List<int>{1,2}),
                new EpisodeCharacters(new List<int>{1,2}),
                new EpisodeCharacters(new List<int>{1,3})
            };
            mock.Setup(m => m.GetAllEpisodesAsync()).ReturnsAsync(episodes);
            mock.Setup(m => m.GetCharacterNamesAsync(It.IsAny<List<int>>()))
                .ReturnsAsync((List<int> ids) => ids.ToDictionary(i => i, i => $"Name{i}"));

            var svc = new TopPairsService(mock.Object);
            var filtered = await svc.GetTopPairsAsync(2, 2, null);
            Assert.All(filtered, p => Assert.Equal(2, p.Episodes));
        }
    }
}