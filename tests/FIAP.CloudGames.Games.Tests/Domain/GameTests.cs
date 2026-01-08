using FIAP.CloudGames.Games.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FIAP.CloudGames.Games.Tests.Domain
{
    public class GameTests
    {
        [Fact]
        public void Create_Game_Should_Generate_Id_And_Set_Properties()
        {
            // Arrange
            var title = "Elden Ring";
            var price = 299.90m;
            var description = "Soulslike RPG";
            var releaseDate = new DateTime(2022, 02, 25);
            var developer = "FromSoftware";
            var publisher = "Bandai Namco";

            // Act
            var game = new Game(
                title,
                price,
                description,
                releaseDate,
                developer,
                publisher
            );

            // Assert
            Assert.NotEqual(Guid.Empty, game.Id);
            Assert.Equal(title, game.Title);
            Assert.Equal(price, game.Price);
            Assert.Equal(description, game.Description);
            Assert.Equal(releaseDate, game.ReleaseDate);
            Assert.Equal(developer, game.Developer);
            Assert.Equal(publisher, game.Publisher);
        }
    }
}
