using Business.Models;
using DataAccess;

namespace Tests.Repositories
{
    [TestFixture]
    public class HighScoreRepositoryTests
    {
        private const string TestFilePath = "test_highscores.json";
        private JsonHighScoreRepository _repository;
        
        [SetUp]
        public void Setup()
        {
            // Create a new repository for each test
            _repository = new JsonHighScoreRepository(TestFilePath);
            
            // Ensure the file is empty
            if (System.IO.File.Exists(TestFilePath))
            {
                System.IO.File.Delete(TestFilePath);
            }
        }
        
        [TearDown]
        public void TearDown()
        {
            // Clean up after tests
            if (System.IO.File.Exists(TestFilePath))
            {
                System.IO.File.Delete(TestFilePath);
            }
        }
        
        [Test]
        public void GetTopHighScores_WhenEmpty_ReturnsEmptyList()
        {
            // Act
            var scores = _repository.GetTopHighScores(10);
            
            // Assert
            Assert.IsNotNull(scores);
            Assert.AreEqual(0, scores.Count);
        }
        
        [Test]
        public void SaveHighScore_AddsHighScore()
        {
            // Arrange
            var highScore = new HighScore("Test Player", 1000, 10, 5, TimeSpan.FromSeconds(20));
            
            // Act
            _repository.SaveHighScore(highScore);
            var scores = _repository.GetTopHighScores(10);
            
            // Assert
            Assert.AreEqual(1, scores.Count);
            Assert.AreEqual(highScore.PlayerName, scores[0].PlayerName);
            Assert.AreEqual(highScore.Score, scores[0].Score);
        }
        
        [Test]
        public void SaveHighScore_WithMultipleScores_SortsDescendingByScore()
        {
            // Arrange
            var highScore1 = new HighScore("Player 1", 1000, 10, 5, TimeSpan.FromSeconds(20));
            var highScore2 = new HighScore("Player 2", 1500, 10, 3, TimeSpan.FromSeconds(15));
            var highScore3 = new HighScore("Player 3", 800, 8, 6, TimeSpan.FromSeconds(25));
            
            // Act
            _repository.SaveHighScore(highScore1);
            _repository.SaveHighScore(highScore2);
            _repository.SaveHighScore(highScore3);
            var scores = _repository.GetTopHighScores(10);
            
            // Assert
            Assert.AreEqual(3, scores.Count);
            Assert.AreEqual(highScore2.PlayerName, scores[0].PlayerName); // Highest score first
            Assert.AreEqual(highScore1.PlayerName, scores[1].PlayerName);
            Assert.AreEqual(highScore3.PlayerName, scores[2].PlayerName);
        }
        
        [Test]
        public void SaveHighScore_WithMoreThanMaxScores_KeepsOnlyTopScores()
        {
            // Arrange
            var maxHighScores = 3;
            var repository = new JsonHighScoreRepository(TestFilePath, maxHighScores);
            
            // Create 5 high scores with varying scores
            var highScores = new List<HighScore>
            {
                new HighScore("Player 1", 1000, 10, 5, TimeSpan.FromSeconds(20)),
                new HighScore("Player 2", 1500, 10, 3, TimeSpan.FromSeconds(15)),
                new HighScore("Player 3", 800, 8, 6, TimeSpan.FromSeconds(25)),
                new HighScore("Player 4", 1200, 12, 7, TimeSpan.FromSeconds(30)),
                new HighScore("Player 5", 500, 6, 4, TimeSpan.FromSeconds(10))
            };
            
            // Expected top 3 scores (sorted by score)
            var expectedTopPlayers = new[] { "Player 2", "Player 4", "Player 1" };
            
            // Act
            foreach (var score in highScores)
            {
                repository.SaveHighScore(score);
            }
            var scores = repository.GetTopHighScores(10);
            
            // Assert
            Assert.AreEqual(maxHighScores, scores.Count);
            for (int i = 0; i < maxHighScores; i++)
            {
                Assert.AreEqual(expectedTopPlayers[i], scores[i].PlayerName);
            }
        }
        
        [Test]
        public void IsHighScore_WithEmptyRepository_ReturnsTrue()
        {
            // Act
            bool isHighScore = _repository.IsHighScore(100);
            
            // Assert
            Assert.IsTrue(isHighScore);
        }
        
        [Test]
        public void IsHighScore_WithFewerThanMaxScores_ReturnsTrue()
        {
            // Arrange
            var maxHighScores = 3;
            var repository = new JsonHighScoreRepository(TestFilePath, maxHighScores);
            
            // Add 2 scores (less than max)
            repository.SaveHighScore(new HighScore("Player 1", 1000, 10, 5, TimeSpan.FromSeconds(20)));
            repository.SaveHighScore(new HighScore("Player 2", 1500, 10, 3, TimeSpan.FromSeconds(15)));
            
            // Act
            bool isHighScore = repository.IsHighScore(100); // Even a low score is a high score if we have fewer than max
            
            // Assert
            Assert.IsTrue(isHighScore);
        }
        
        [Test]
        public void IsHighScore_WithFullRepositoryAndHigherScore_ReturnsTrue()
        {
            // Arrange
            var maxHighScores = 3;
            var repository = new JsonHighScoreRepository(TestFilePath, maxHighScores);
            
            // Add max number of scores
            repository.SaveHighScore(new HighScore("Player 1", 1000, 10, 5, TimeSpan.FromSeconds(20)));
            repository.SaveHighScore(new HighScore("Player 2", 1500, 10, 3, TimeSpan.FromSeconds(15)));
            repository.SaveHighScore(new HighScore("Player 3", 800, 8, 6, TimeSpan.FromSeconds(25)));
            
            // Act
            bool isHighScore = repository.IsHighScore(900); // Higher than the lowest score (800)
            
            // Assert
            Assert.IsTrue(isHighScore);
        }
        
        [Test]
        public void IsHighScore_WithFullRepositoryAndLowerScore_ReturnsFalse()
        {
            // Arrange
            var maxHighScores = 3;
            var repository = new JsonHighScoreRepository(TestFilePath, maxHighScores);
            
            // Add max number of scores
            repository.SaveHighScore(new HighScore("Player 1", 1000, 10, 5, TimeSpan.FromSeconds(20)));
            repository.SaveHighScore(new HighScore("Player 2", 1500, 10, 3, TimeSpan.FromSeconds(15)));
            repository.SaveHighScore(new HighScore("Player 3", 800, 8, 6, TimeSpan.FromSeconds(25)));
            
            // Act
            bool isHighScore = repository.IsHighScore(700); // Lower than the lowest score (800)
            
            // Assert
            Assert.IsFalse(isHighScore);
        }
    }
}