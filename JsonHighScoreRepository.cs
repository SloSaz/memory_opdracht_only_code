using System.Text.Json;
using Business;
using Business.Models;

namespace DataAccess;

/// <summary>
    /// Repository implementation that stores high scores in a JSON file
    /// </summary>
    public class JsonHighScoreRepository : IHighScoreRepository
    {
        private readonly string _filePath;
        private readonly int _maxHighScores;
        private List<HighScore> _highScores;

        public JsonHighScoreRepository(string filePath, int maxHighScores = 10)
        {
            _filePath = filePath ?? throw new ArgumentNullException(nameof(filePath));
            _maxHighScores = maxHighScores;
            _highScores = LoadHighScores();
        }

        private List<HighScore> LoadHighScores()
        {
            try
            {
                if (File.Exists(_filePath))
                {
                    string json = File.ReadAllText(_filePath);
                    return JsonSerializer.Deserialize<List<HighScore>>(json) ?? new List<HighScore>();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading high scores: {ex.Message}");
            }

            return new List<HighScore>();
        }

        private void SaveHighScores()
        {
            try
            {
                string directoryPath = Path.GetDirectoryName(_filePath);
                if (!string.IsNullOrEmpty(directoryPath) && !Directory.Exists(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                }

                string json = JsonSerializer.Serialize(_highScores, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(_filePath, json);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving high scores: {ex.Message}");
            }
        }

        public List<HighScore> GetTopHighScores(int count)
        {
            return _highScores
                .OrderByDescending(h => h.Score)
                .Take(count)
                .ToList();
        }

        public bool IsHighScore(int score)
        {
            if (_highScores.Count < _maxHighScores)
                return true;

            return _highScores.Any(h => h.Score < score);
        }

        public void SaveHighScore(HighScore highScore)
        {
            if (highScore == null)
                throw new ArgumentNullException(nameof(highScore));

            // Add the new high score
            _highScores.Add(highScore);

            // Sort high scores by score (descending)
            _highScores = _highScores
                .OrderByDescending(h => h.Score)
                .ToList();

            // Keep only the top scores
            if (_highScores.Count > _maxHighScores)
            {
                _highScores = _highScores
                    .Take(_maxHighScores)
                    .ToList();
            }

            // Save to file
            SaveHighScores();
        }
    }