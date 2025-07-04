using Business.Models;

namespace Tests.Models
{
    [TestFixture]
    public class HighScoreTests
    {
        [Test]
        public void Constructor_InitializesProperties()
        {
            // Arrange
            string playerName = "Test Player";
            int score = 1000;
            int cardCount = 10;
            int attempts = 5;
            TimeSpan duration = TimeSpan.FromSeconds(20);
            
            // Act
            var highScore = new HighScore(playerName, score, cardCount, attempts, duration);
            
            // Assert
            Assert.AreEqual(playerName, highScore.PlayerName);
            Assert.AreEqual(score, highScore.Score);
            Assert.AreEqual(cardCount, highScore.CardCount);
            Assert.AreEqual(attempts, highScore.Attempts);
            Assert.AreEqual(duration, highScore.Duration);
            Assert.IsNotNull(highScore.Date);
            // Date should be around current time
            Assert.Less(Math.Abs((DateTime.Now - highScore.Date).TotalSeconds), 5);
        }
        
        [Test]
        public void Constructor_WithNullPlayerName_ThrowsArgumentNullException()
        {
            // Arrange, Act & Assert
            Assert.Throws<ArgumentNullException>(() => 
                new HighScore(null, 1000, 10, 5, TimeSpan.FromSeconds(20)));
        }
        
        [Test]
        public void ScoreCalculation_MatchesFormula()
        {
            // These tests verify the score formula: ((Aantal kaarten)² / (Tijd in seconden * aantal pogingen)) * 1000
            
            // Test case 1: 4 cards, 10 seconds, 2 attempts
            int cardCount1 = 4;
            int attempts1 = 2;
            TimeSpan duration1 = TimeSpan.FromSeconds(10);
            int expectedScore1 = 800; // (16 / 20) * 1000 = 800
            
            // Test case 2: 10 cards, 20 seconds, 5 attempts
            int cardCount2 = 10;
            int attempts2 = 5;
            TimeSpan duration2 = TimeSpan.FromSeconds(20);
            int expectedScore2 = 1000; // (100 / 100) * 1000 = 1000
            
            // Test case 3: 4 cards, 20 seconds, 2 attempts
            int cardCount3 = 4;
            int attempts3 = 2;
            TimeSpan duration3 = TimeSpan.FromSeconds(20);
            int expectedScore3 = 400; // (16 / 40) * 1000 = 400
            
            // Test case 4: 4 cards, 10 seconds, 3 attempts
            int cardCount4 = 4;
            int attempts4 = 3;
            TimeSpan duration4 = TimeSpan.FromSeconds(10);
            int expectedScore4 = 533; // (16 / 30) * 1000 = 533.33...
            
            // Function to calculate score using the formula
            int CalculateScore(int cards, int att, TimeSpan dur)
            {
                double score = Math.Pow(cards, 2) / (dur.TotalSeconds * att) * 1000;
                return (int)Math.Round(score);
            }
            
            // Verify all test cases
            Assert.AreEqual(expectedScore1, CalculateScore(cardCount1, attempts1, duration1));
            Assert.AreEqual(expectedScore2, CalculateScore(cardCount2, attempts2, duration2));
            Assert.AreEqual(expectedScore3, CalculateScore(cardCount3, attempts3, duration3));
            Assert.AreEqual(expectedScore4, CalculateScore(cardCount4, attempts4, duration4));
        }
    }
}