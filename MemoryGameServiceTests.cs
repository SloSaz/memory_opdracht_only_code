using Business;
using Business.Events;
using Business.Exceptions;
using Business.Models;
using Business.Services;
using Moq;
using NUnit.Framework;

namespace Tests.Services;

    [TestFixture]
    public class MemoryGameServiceTests
    {
        private Mock<IHighScoreRepository> _mockRepository;
        private Player _player;
        private const int DefaultPairCount = 5;

        [SetUp]
        public void Setup()
        {
            _mockRepository = new Mock<IHighScoreRepository>();
            _player = new Player("Test Player");
        }

        [Test]
        public void Constructor_WithValidParams_CreatesGameWithCorrectCardCount()
        {
            // Arrange & Act
            var game = new MemoryGameService(_player, DefaultPairCount, _mockRepository.Object);

            // Assert
            Assert.AreEqual(DefaultPairCount * 2, game.CardCount);
            Assert.AreEqual(DefaultPairCount, game.RemainingPairs);
            Assert.AreEqual(0, game.Attempts);
        }

        [Test]
        public void Constructor_WithInvalidPairCount_ThrowsArgumentException()
        {
            // Arrange, Act & Assert
            Assert.Throws<ArgumentException>(() => 
                new MemoryGameService(_player, 1, _mockRepository.Object));
        }

        [Test]
        public void Constructor_WithNullPlayer_ThrowsArgumentNullException()
        {
            // Arrange, Act & Assert
            Assert.Throws<ArgumentNullException>(() => 
                new MemoryGameService(null, DefaultPairCount, _mockRepository.Object));
        }

        [Test]
        public void Constructor_WithNullRepository_ThrowsArgumentNullException()
        {
            // Arrange, Act & Assert
            Assert.Throws<ArgumentNullException>(() => 
                new MemoryGameService(_player, DefaultPairCount, null));
        }

        [Test]
        public void FlipCard_WithValidCardIndex_FlipsCard()
        {
            // Arrange
            var game = new MemoryGameService(_player, DefaultPairCount, _mockRepository.Object);
            bool eventRaised = false;
            game.CardFlipped += (sender, args) => eventRaised = true;

            // Act
            game.FlipCard(0);

            // Assert
            Assert.IsTrue(game.Cards[0].IsFaceUp);
            Assert.IsTrue(eventRaised);
        }

        [Test]
        public void FlipCard_WithInvalidCardIndex_ThrowsArgumentOutOfRangeException()
        {
            // Arrange
            var game = new MemoryGameService(_player, DefaultPairCount, _mockRepository.Object);

            // Act & Assert
            Assert.Throws<ArgumentOutOfRangeException>(() => game.FlipCard(-1));
            Assert.Throws<ArgumentOutOfRangeException>(() => game.FlipCard(game.CardCount));
        }

        [Test]
        public void FlipCard_OnMatchedCard_ThrowsInvalidGameOperationException()
        {
            // Arrange
            var game = new MemoryGameService(_player, DefaultPairCount, _mockRepository.Object);
            
            // Find a pair
            string symbol = game.Cards[0].Symbol;
            int matchingCardIndex = game.Cards.FindIndex(1, c => c.Symbol == symbol);
            
            // Manually set cards as matched
            game.Cards[0].SetMatched();
            game.Cards[matchingCardIndex].SetMatched();

            // Act & Assert
            Assert.Throws<InvalidGameOperationException>(() => game.FlipCard(0));
        }

        [Test]
        public void FlipCard_OnAlreadyFaceUpCard_ThrowsInvalidGameOperationException()
        {
            // Arrange
            var game = new MemoryGameService(_player, DefaultPairCount, _mockRepository.Object);
            game.FlipCard(0); // Flip card face up

            // Act & Assert
            Assert.Throws<InvalidGameOperationException>(() => game.FlipCard(0));
        }

        [Test]
        public void FlipCard_WithMatchingCards_SetsCardsAsMatched()
        {
            // Arrange
            var game = new MemoryGameService(_player, 2, _mockRepository.Object); // Use smaller game for easier testing
            
            // Find a pair
            string symbol = game.Cards[0].Symbol;
            int matchingCardIndex = game.Cards.FindIndex(1, c => c.Symbol == symbol);
            
            bool matchFoundEventRaised = false;
            game.MatchFound += (sender, args) => matchFoundEventRaised = true;

            // Act
            game.FlipCard(0);
            game.FlipCard(matchingCardIndex);

            // Assert
            Assert.IsTrue(game.Cards[0].IsMatched);
            Assert.IsTrue(game.Cards[matchingCardIndex].IsMatched);
            Assert.AreEqual(1, game.Attempts);
            Assert.IsTrue(matchFoundEventRaised);
        }

        [Test]
        public void FlipCard_WithNonMatchingCards_IncreasesAttempts()
        {
            // Arrange
            var game = new MemoryGameService(_player, 3, _mockRepository.Object); // Use small game with at least 2 different symbols
            
            // Find two cards with different symbols
            string symbol1 = game.Cards[0].Symbol;
            int nonMatchingCardIndex = game.Cards.FindIndex(c => c.Symbol != symbol1);
            
            bool matchFailedEventRaised = false;
            game.MatchFailed += (sender, args) => matchFailedEventRaised = true;

            // Act
            game.FlipCard(0);
            game.FlipCard(nonMatchingCardIndex);

            // Assert
            Assert.IsFalse(game.Cards[0].IsMatched);
            Assert.IsFalse(game.Cards[nonMatchingCardIndex].IsMatched);
            Assert.AreEqual(1, game.Attempts);
            Assert.IsTrue(matchFailedEventRaised);
        }

        [Test]
        public void FlipCard_CompletingGame_RaisesGameCompletedEvent()
        {
            // Arrange
            var game = new MemoryGameService(_player, 2, _mockRepository.Object); // Small game with 2 pairs
            _mockRepository.Setup(r => r.IsHighScore(It.IsAny<int>())).Returns(true);
            
            bool gameCompletedEventRaised = false;
            GameCompletedEventArgs completedEventArgs = null;
            
            game.GameCompleted += (sender, args) => 
            {
                gameCompletedEventRaised = true;
                completedEventArgs = args;
            };

            // Act - Find and flip all matching pairs
            for (int i = 0; i < game.CardCount / 2; i++)
            {
                // Find cards with the same symbol
                var remainingCards = game.Cards.Where(c => !c.IsMatched).ToList();
                string symbol = remainingCards[0].Symbol;
                int firstCardIndex = game.Cards.FindIndex(c => c.Symbol == symbol && !c.IsMatched);
                int secondCardIndex = game.Cards.FindIndex(firstCardIndex + 1, c => c.Symbol == symbol && !c.IsMatched);
                
                game.FlipCard(firstCardIndex);
                game.FlipCard(secondCardIndex);
            }

            // Assert
            Assert.AreEqual(0, game.RemainingPairs);
            Assert.IsTrue(gameCompletedEventRaised);
            Assert.IsNotNull(completedEventArgs);
            Assert.AreEqual(_player, completedEventArgs.Player);
            Assert.IsTrue(completedEventArgs.IsHighScore);
            
            // Verify high score was saved
            _mockRepository.Verify(r => r.SaveHighScore(It.IsAny<HighScore>()), Times.Once);
        }

        [Test]
        public void FlipCard_CompletingGame_DoesNotSaveHighScoreIfNotHighEnough()
        {
            // Arrange
            var game = new MemoryGameService(_player, 2, _mockRepository.Object); // Small game with 2 pairs
            _mockRepository.Setup(r => r.IsHighScore(It.IsAny<int>())).Returns(false);
            
            // Act - Find and flip all matching pairs
            for (int i = 0; i < game.CardCount / 2; i++)
            {
                // Find cards with the same symbol
                var remainingCards = game.Cards.Where(c => !c.IsMatched).ToList();
                string symbol = remainingCards[0].Symbol;
                int firstCardIndex = game.Cards.FindIndex(c => c.Symbol == symbol && !c.IsMatched);
                int secondCardIndex = game.Cards.FindIndex(firstCardIndex + 1, c => c.Symbol == symbol && !c.IsMatched);
                
                game.FlipCard(firstCardIndex);
                game.FlipCard(secondCardIndex);
            }

            // Assert
            _mockRepository.Verify(r => r.SaveHighScore(It.IsAny<HighScore>()), Times.Never);
        }

        [Test]
        public void FlipCardBack_WithValidCardIndex_FlipsCardBack()
        {
            // Arrange
            var game = new MemoryGameService(_player, DefaultPairCount, _mockRepository.Object);
            game.FlipCard(0); // Flip card face up
            Assert.IsTrue(game.Cards[0].IsFaceUp);

            // Act
            game.FlipCardBack(0);

            // Assert
            Assert.IsFalse(game.Cards[0].IsFaceUp);
        }

        [Test]
        public void FlipCardBack_WithInvalidCardIndex_ThrowsArgumentOutOfRangeException()
        {
            // Arrange
            var game = new MemoryGameService(_player, DefaultPairCount, _mockRepository.Object);

            // Act & Assert
            Assert.Throws<ArgumentOutOfRangeException>(() => game.FlipCardBack(-1));
            Assert.Throws<ArgumentOutOfRangeException>(() => game.FlipCardBack(game.CardCount));
        }
    }
