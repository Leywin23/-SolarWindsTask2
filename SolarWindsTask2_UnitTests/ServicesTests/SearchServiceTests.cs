using System;
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
    public class SearchServiceTests
    {
        [Fact]
        public async Task SearchAsync_TrimsTerm_AndPassesTrimmedValueToClient()
        {
            var mockRm = new Mock<IRickAndMortyClient>();
            mockRm.Setup(m => m.SearchAllAsync("rick"))
                .ReturnsAsync((
                    new List<RmItem>(),
                    new List<RmItem>(),
                    new List<RmItem>()
                ));

            var svc = new SearchService(mockRm.Object);

            await svc.SearchAsync("   rick   ", null);

            mockRm.Verify(m => m.SearchAllAsync("rick"), Times.Once);
            mockRm.VerifyNoOtherCalls();
        }

        [Theory]
        [InlineData("   ")]
        [InlineData("\t\r\n")]
        public async Task SearchAsync_Throws_OnWhitespaceOnlyTerm(string term)
        {
            var mockRm = new Mock<IRickAndMortyClient>();
            var svc = new SearchService(mockRm.Object);

            await Assert.ThrowsAsync<ArgumentException>(() => svc.SearchAsync(term, null));

            mockRm.Verify(m => m.SearchAllAsync(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task SearchAsync_MapsAllItems_ToResultDto_WithCorrectTypeNameUrl()
        {
            var mockRm = new Mock<IRickAndMortyClient>();
            mockRm.Setup(m => m.SearchAllAsync("rick"))
                .ReturnsAsync((
                    new List<RmItem> { new RmItem { Name = "Rick Sanchez", Url = "https://.../character/1" } },
                    new List<RmItem> { new RmItem { Name = "Earth (C-137)", Url = "https://.../location/1" } },
                    new List<RmItem> { new RmItem { Name = "Pilot", Url = "https://.../episode/1" } }
                ));

            var svc = new SearchService(mockRm.Object);

            var res = await svc.SearchAsync("rick", null);

            Assert.Equal(3, res.Count);

            var character = res.Single(r => r.Type == "character");
            Assert.Equal("Rick Sanchez", character.Name);
            Assert.Equal("https://.../character/1", character.Url);

            var location = res.Single(r => r.Type == "location");
            Assert.Equal("Earth (C-137)", location.Name);
            Assert.Equal("https://.../location/1", location.Url);

            var episode = res.Single(r => r.Type == "episode");
            Assert.Equal("Pilot", episode.Name);
            Assert.Equal("https://.../episode/1", episode.Url);
        }

        [Fact]
        public async Task SearchAsync_PreservesOrder_CharactersThenLocationsThenEpisodes()
        {
            var mockRm = new Mock<IRickAndMortyClient>();
            mockRm.Setup(m => m.SearchAllAsync("rick"))
                .ReturnsAsync((
                    new List<RmItem>
                    {
                        new RmItem { Name = "A", Url = "c1" },
                        new RmItem { Name = "B", Url = "c2" },
                    },
                    new List<RmItem>
                    {
                        new RmItem { Name = "C", Url = "l1" },
                    },
                    new List<RmItem>
                    {
                        new RmItem { Name = "D", Url = "e1" },
                        new RmItem { Name = "E", Url = "e2" },
                    }
                ));

            var svc = new SearchService(mockRm.Object);

            var res = await svc.SearchAsync("rick", null);

            Assert.Equal(
                new[] { "character", "character", "location", "episode", "episode" },
                res.Select(r => r.Type).ToArray()
            );

            Assert.Equal(
                new[] { "A", "B", "C", "D", "E" },
                res.Select(r => r.Name).ToArray()
            );
        }

        [Fact]
        public async Task SearchAsync_WhenLimitIsNull_ReturnsAll()
        {
            var mockRm = new Mock<IRickAndMortyClient>();
            mockRm.Setup(m => m.SearchAllAsync("rick"))
                .ReturnsAsync((
                    new List<RmItem> { new RmItem { Name = "Rick", Url = "c1" } },
                    new List<RmItem> { new RmItem { Name = "Earth", Url = "l1" } },
                    new List<RmItem> { new RmItem { Name = "Pilot", Url = "e1" } }
                ));

            var svc = new SearchService(mockRm.Object);

            var res = await svc.SearchAsync("rick", null);

            Assert.Equal(3, res.Count);
        }

        [Fact]
        public async Task SearchAsync_WhenLimitIsZero_ReturnsEmptyList()
        {
            var mockRm = new Mock<IRickAndMortyClient>();
            mockRm.Setup(m => m.SearchAllAsync("rick"))
                .ReturnsAsync((
                    new List<RmItem> { new RmItem { Name = "Rick", Url = "c1" } },
                    new List<RmItem> { new RmItem { Name = "Earth", Url = "l1" } },
                    new List<RmItem> { new RmItem { Name = "Pilot", Url = "e1" } }
                ));

            var svc = new SearchService(mockRm.Object);

            var res = await svc.SearchAsync("rick", 0);

            Assert.NotNull(res);
            Assert.Empty(res);
        }

        [Fact]
        public async Task SearchAsync_WhenLimitIsNegative_DoesNotApplyLimit_ReturnsAll()
        {
            var mockRm = new Mock<IRickAndMortyClient>();
            mockRm.Setup(m => m.SearchAllAsync("rick"))
                .ReturnsAsync((
                    new List<RmItem> { new RmItem { Name = "Rick", Url = "c1" } },
                    new List<RmItem> { new RmItem { Name = "Earth", Url = "l1" } },
                    new List<RmItem> { new RmItem { Name = "Pilot", Url = "e1" } }
                ));

            var svc = new SearchService(mockRm.Object);

            var res = await svc.SearchAsync("rick", -1);

            Assert.Equal(3, res.Count);
        }

        [Fact]
        public async Task SearchAsync_CallsClientExactlyOnce()
        {
            var mockRm = new Mock<IRickAndMortyClient>();
            mockRm.Setup(m => m.SearchAllAsync("rick"))
                .ReturnsAsync((
                    new List<RmItem>(),
                    new List<RmItem>(),
                    new List<RmItem>()
                ));

            var svc = new SearchService(mockRm.Object);

            await svc.SearchAsync("rick", 10);

            mockRm.Verify(m => m.SearchAllAsync("rick"), Times.Once);
            mockRm.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task SearchAsync_WhenClientReturnsEmptyLists_ReturnsEmpty()
        {
            var mockRm = new Mock<IRickAndMortyClient>();
            mockRm.Setup(m => m.SearchAllAsync("rick"))
                .ReturnsAsync((
                    new List<RmItem>(),
                    new List<RmItem>(),
                    new List<RmItem>()
                ));

            var svc = new SearchService(mockRm.Object);

            var res = await svc.SearchAsync("rick", null);

            Assert.NotNull(res);
            Assert.Empty(res);
        }
    }
}