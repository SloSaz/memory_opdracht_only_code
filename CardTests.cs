using Business.Models;

namespace Tests.Models
{
    [TestFixture]
    public class CardTests
    {
        [Test]
        public void Constructor_InitializesProperties()
        {
            // Arrange & Act
            int id = 1;
            string symbol = "A";
            var card = new Card(id, symbol);

            // Assert
            Assert.AreEqual(id, card.Id);
            Assert.AreEqual(symbol, card.Symbol);
            Assert.IsFalse(card.IsFaceUp);
            Assert.IsFalse(card.IsMatched);
        }

        [Test]
        public void Flip_TogglesIsFaceUp()
        {
            // Arrange
            var card = new Card(1, "A");
            Assert.IsFalse(card.IsFaceUp);

            // Act
            card.Flip();

            // Assert
            Assert.IsTrue(card.IsFaceUp);

            // Act again
            card.Flip();

            // Assert
            Assert.IsFalse(card.IsFaceUp);
        }

        [Test]
        public void SetMatched_SetsIsMatchedAndIsFaceUp()
        {
            // Arrange
            var card = new Card(1, "A");
            Assert.IsFalse(card.IsMatched);
            Assert.IsFalse(card.IsFaceUp);

            // Act
            card.SetMatched();

            // Assert
            Assert.IsTrue(card.IsMatched);
            Assert.IsTrue(card.IsFaceUp);
        }

        [Test]
        public void Reset_ResetsStateToInitial()
        {
            // Arrange
            var card = new Card(1, "A");
            card.Flip(); // Make face up
            card.SetMatched(); // Set as matched
            Assert.IsTrue(card.IsFaceUp);
            Assert.IsTrue(card.IsMatched);

            // Act
            card.Reset();

            // Assert
            Assert.IsFalse(card.IsFaceUp);
            Assert.IsFalse(card.IsMatched);
        }
    }
}