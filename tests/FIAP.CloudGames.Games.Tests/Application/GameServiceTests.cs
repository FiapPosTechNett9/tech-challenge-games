using FIAP.CloudGames.Games.Application.Dtos;
using FIAP.CloudGames.Games.Application.Interfaces;
using FIAP.CloudGames.Games.Application.Services;
using FIAP.CloudGames.Games.Domain.Entities;
using FIAP.CloudGames.Games.Domain.Interfaces.Repositories;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FIAP.CloudGames.Games.Tests.Application
{
    public class GameServiceTests
    {
        [Fact]
        public async Task CreateAsync_Should_Save_Game_And_Index_In_Elasticsearch()
        {
            // Arrange
            var repositoryMock = new Mock<IGameRepository>();
            var searchMock = new Mock<IGameSearchService>();

            repositoryMock
                .Setup(r => r.AddAsync(It.IsAny<Game>()))
                .Returns(Task.CompletedTask);

            searchMock
                .Setup(s => s.IndexAsync(It.IsAny<Game>(), default))
                .Returns(Task.CompletedTask);

            var service = new GameService(
                repositoryMock.Object,
                searchMock.Object
            );

            var dto = new CreateGameDto
            {
                Title = "God of War",
                Price = 349.90m,
                Description = "Action Adventure",
                ReleaseDate = new DateTime(2022, 11, 9),
                Developer = "Santa Monica Studio",
                Publisher = "Sony"
            };

            // Act
            var game = await service.CreateAsync(dto);

            // Assert
            repositoryMock.Verify(r => r.AddAsync(It.IsAny<Game>()), Times.Once);
            searchMock.Verify(s => s.IndexAsync(It.IsAny<Game>(), default), Times.Once);

            Assert.NotNull(game);
            Assert.Equal(dto.Title, game.Title);
        }
    }
}
