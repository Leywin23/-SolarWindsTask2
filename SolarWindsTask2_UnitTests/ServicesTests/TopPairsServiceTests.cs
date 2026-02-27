using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Moq;
using SolarWindsTask2.Dtos;
using SolarWindsTask2.Interfaces;
using SolarWindsTask2.Models;
using SolarWindsTask2.Services;
using Xunit;

namespace SolarWindsTask2_UnitTests.Services
{
    public class TopPairsServiceTests
    {
        private static Mock<IRickAndMortyClient> BuildMock(
            List<EpisodeCharacters> episodes,
            Func<List<int>, Dictionary<int, string>>? namesFactory = null)
        {
            var mock = new Mock<IRickAndMortyClient>();
            mock.Setup(m => m.GetAllEpisodesAsync()).ReturnsAsync(episodes);

            namesFactory ??= (ids) => ids.ToDictionary(i => i, i => $"Name{i}");

            mock.Setup(m => m.GetCharacterNamesAsync(It.IsAny<List<int>>()))
                .ReturnsAsync((List<int> ids) => namesFactory(ids));

            return mock;
        }

        [Fact]
        public async Task GetTopPairsAsync_ComputesPairCounts_WithDistinctPerEpisode()
        {
            var episodes = new List<EpisodeCharacters>
            {
                new EpisodeCharacters(new List<int>{ 1, 1, 2, 2 }),
                new EpisodeCharacters(new List<int>{ 1, 2 })
            };

            var mock = BuildMock(episodes);
            var svc = new TopPairsService(mock.Object);

            var res = await svc.GetTopPairsAsync(null, null, null);

            var pair12 = res.Single(p =>
                p.Character1.Url.EndsWith("/1") && p.Character2.Url.EndsWith("/2"));

            Assert.Equal(2, pair12.Episodes);
        }

        [Fact]
        public async Task GetTopPairsAsync_OrdersByEpisodesDesc_ThenByA_ThenByB()
        {
            var episodes = new List<EpisodeCharacters>
            {
                new EpisodeCharacters(new List<int>{ 2, 1 }),
                new EpisodeCharacters(new List<int>{ 3, 1 }), 
                new EpisodeCharacters(new List<int>{ 3, 2 }),
            };

            var mock = BuildMock(episodes);
            var svc = new TopPairsService(mock.Object);

            var res = await svc.GetTopPairsAsync(null, null, null);

            Assert.Equal(3, res.Count);

            Assert.All(res, p => Assert.Equal(1, p.Episodes));
            Assert.True(res[0].Character1.Url.EndsWith("/1") && res[0].Character2.Url.EndsWith("/2"));
            Assert.True(res[1].Character1.Url.EndsWith("/1") && res[1].Character2.Url.EndsWith("/3"));
            Assert.True(res[2].Character1.Url.EndsWith("/2") && res[2].Character2.Url.EndsWith("/3"));
        }

        [Fact]
        public async Task GetTopPairsAsync_AppliesLimit_DefaultIs20()
        {
            var ids = Enumerable.Range(1, 10).ToList();
            var episodes = new List<EpisodeCharacters> { new EpisodeCharacters(ids) };

            var mock = BuildMock(episodes);
            var svc = new TopPairsService(mock.Object);

            var resDefault = await svc.GetTopPairsAsync(null, null, null);
            Assert.Equal(20, resDefault.Count);

            var res10 = await svc.GetTopPairsAsync(null, null, 10);
            Assert.Equal(10, res10.Count);
        }

        [Fact]
        public async Task GetTopPairsAsync_WhenLimitIsZero_ReturnsEmpty_AndDoesNotCallNames()
        {
            var episodes = new List<EpisodeCharacters>
            {
                new EpisodeCharacters(new List<int>{ 1,2,3 })
            };

            var mock = BuildMock(episodes);
            var svc = new TopPairsService(mock.Object);

            var res = await svc.GetTopPairsAsync(null, null, 0);

            Assert.Empty(res);

            mock.Verify(m => m.GetCharacterNamesAsync(It.IsAny<List<int>>()), Times.Never);
        }

        [Fact]
        public async Task GetTopPairsAsync_WhenLimitIsNegative_TreatsAsZero_ReturnsEmpty()
        {
            var episodes = new List<EpisodeCharacters>
            {
                new EpisodeCharacters(new List<int>{ 1,2,3 })
            };

            var mock = BuildMock(episodes);
            var svc = new TopPairsService(mock.Object);

            var res = await svc.GetTopPairsAsync(null, null, -5);

            Assert.Empty(res);
            mock.Verify(m => m.GetCharacterNamesAsync(It.IsAny<List<int>>()), Times.Never);
        }

        [Fact]
        public async Task GetTopPairsAsync_WhenNoPairs_ReturnsEmpty_AndDoesNotCallNames()
        {
            var episodes = new List<EpisodeCharacters>
            {
                new EpisodeCharacters(new List<int>()),
                new EpisodeCharacters(new List<int>{ 1 }),
                new EpisodeCharacters(new List<int>{ 2 })
            };

            var mock = BuildMock(episodes);
            var svc = new TopPairsService(mock.Object);

            var res = await svc.GetTopPairsAsync(null, null, null);

            Assert.Empty(res);
            mock.Verify(m => m.GetCharacterNamesAsync(It.IsAny<List<int>>()), Times.Never);
        }

        [Fact]
        public async Task GetTopPairsAsync_CallsGetCharacterNames_WithIdsFromTopOnly_Distinct()
        {
            var episodes = new List<EpisodeCharacters>
            {
                new EpisodeCharacters(new List<int>{ 1,2,3 }),
                new EpisodeCharacters(new List<int>{ 2,1 })   
            };

            List<int>? capturedIds = null;

            var mock = new Mock<IRickAndMortyClient>();
            mock.Setup(m => m.GetAllEpisodesAsync()).ReturnsAsync(episodes);
            mock.Setup(m => m.GetCharacterNamesAsync(It.IsAny<List<int>>()))
                .Callback<List<int>>(ids => capturedIds = ids)
                .ReturnsAsync((List<int> ids) => ids.ToDictionary(i => i, i => $"Name{i}"));

            var svc = new TopPairsService(mock.Object);

            var res = await svc.GetTopPairsAsync(null, null, 1);

            Assert.Single(res);
            mock.Verify(m => m.GetCharacterNamesAsync(It.IsAny<List<int>>()), Times.Once);

            Assert.NotNull(capturedIds);
            Assert.Equal(2, capturedIds!.Distinct().Count());
            Assert.Contains(1, capturedIds);
            Assert.Contains(2, capturedIds);
            Assert.DoesNotContain(3, capturedIds);
        }

        [Fact]
        public async Task GetTopPairsAsync_UsesFallbackName_WhenNameMissing()
        {
            var episodes = new List<EpisodeCharacters>
            {
                new EpisodeCharacters(new List<int>{ 10, 20 })
            };

            var mock = BuildMock(
                episodes,
                ids => new Dictionary<int, string> { { 10, "Ten" } }
            );

            var svc = new TopPairsService(mock.Object);

            var res = await svc.GetTopPairsAsync(null, null, null);

            Assert.Single(res);
            Assert.Equal("Ten", res[0].Character1.Name);
            Assert.Equal("#20", res[0].Character2.Name);
        }

        [Fact]
        public async Task GetTopPairsAsync_BuildsCorrectCharacterUrls()
        {
            var episodes = new List<EpisodeCharacters>
            {
                new EpisodeCharacters(new List<int>{ 7, 42 })
            };

            var mock = BuildMock(episodes);
            var svc = new TopPairsService(mock.Object);

            var res = await svc.GetTopPairsAsync(null, null, null);

            Assert.Single(res);
            Assert.Equal("https://rickandmortyapi.com/api/character/7", res[0].Character1.Url);
            Assert.Equal("https://rickandmortyapi.com/api/character/42", res[0].Character2.Url);
        }

        [Fact]
        public async Task GetTopPairsAsync_Throws_WhenMinIsNegative()
        {
            var svc = new TopPairsService(BuildMock(new List<EpisodeCharacters>()).Object);
            await Assert.ThrowsAsync<ArgumentException>(() => svc.GetTopPairsAsync(-1, null, null));
        }

        [Fact]
        public async Task GetTopPairsAsync_Throws_WhenMaxLessThanMin()
        {
            var svc = new TopPairsService(BuildMock(new List<EpisodeCharacters>()).Object);
            await Assert.ThrowsAsync<ArgumentException>(() => svc.GetTopPairsAsync(5, 3, null));
        }

        [Fact]
        public async Task GetTopPairsAsync_AppliesMinMaxFilter_IncludingBounds()
        {

            var episodes = new List<EpisodeCharacters>
            {
                new EpisodeCharacters(new List<int>{1,2,3}),
                new EpisodeCharacters(new List<int>{1,2}),  
                new EpisodeCharacters(new List<int>{1,2}),  
                new EpisodeCharacters(new List<int>{1,3}), 
            };

            var mock = BuildMock(episodes);
            var svc = new TopPairsService(mock.Object);

            var res = await svc.GetTopPairsAsync(2, 3, null);

            Assert.NotEmpty(res);
            Assert.All(res, p => Assert.InRange(p.Episodes, 2, 3));
            Assert.Contains(res, p => p.Episodes == 3);
            Assert.Contains(res, p => p.Episodes == 2);
            Assert.DoesNotContain(res, p => p.Episodes == 1);
        }
    }
}