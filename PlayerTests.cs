using Business.Models;

namespace Tests.Models
{
    [TestFixture]
    public class PlayerTests
    {
        [Test]
        public void Constructor_WithValidName_InitializesName()
        {
            // Arrange
            string name = "Test Player";
            
            // Act
            var player = new Player(name);
            
            // Assert
            Assert.AreEqual(name, player.Name);
        }
        
        [Test]
        public void Constructor_WithNullOrEmptyName_ThrowsArgumentException()
        {
            // Arrange, Act & Assert
            Assert.Throws<ArgumentException>(() => new Player(null));
            Assert.Throws<ArgumentException>(() => new Player(""));
            Assert.Throws<ArgumentException>(() => new Player("   "));
        }
    }
}