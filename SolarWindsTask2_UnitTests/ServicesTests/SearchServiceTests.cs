using System;
using System.Collections.Generic;
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
        public async Task SearchAsync_ReturnsCombinedAndRespectsLimit()
        {
            var mockRm = new Mock<IRickAndMortyClient>();
            mockRm.Setup(m => m.SearchAllAsync("rick"))
                .ReturnsAsync((
                    new List<RmItem> { new RmItem { Name = "Rick Sanchez", Url = "https://.../character/1" } },
                    new List<RmItem> { new RmItem { Name = "Earth (C-137)", Url = "https://.../location/1" } },
                    new List<RmItem> { new RmItem { Name = "Pilot", Url = "https://.../episode/1" } }
                ));

            var svc = new SearchService(mockRm.Object);
            var res = await svc.SearchAsync("rick", 2);

            Assert.Equal(2, res.Count);
            Assert.Contains(res, r => r.Type == "character" && r.Name == "Rick Sanchez");
            Assert.Contains(res, r => r.Type == "location" && r.Name == "Earth (C-137)");
        }

        [Fact]
        public async Task SearchAsync_Throws_OnEmptyTerm()
        {
            var mockRm = new Mock<IRickAndMortyClient>();
            var svc = new SearchService(mockRm.Object);
            await Assert.ThrowsAsync<ArgumentException>(() => svc.SearchAsync("", null));
        }
    }
}