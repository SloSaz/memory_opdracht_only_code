using Business;
using Business.Models;

namespace DataAccess;

/// <summary>
/// Repository implementation that uses an SQL database for high scores
/// </summary>
public class SqlHighScoreRepository : IHighScoreRepository
{
    // This would be implemented with actual database access in a real project
    // For this example, we'll just use an in-memory list to simulate a database

    private readonly List<HighScore> _highScores = new List<HighScore>();
    private readonly int _maxHighScores;

    public SqlHighScoreRepository(int maxHighScores = 10)
    {
        _maxHighScores = maxHighScores;
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
        var sortedHighScores = _highScores
            .OrderByDescending(h => h.Score)
            .ToList();

        // Keep only the top scores
        if (sortedHighScores.Count > _maxHighScores)
        {
            var highScoresToRemove = sortedHighScores
                .Skip(_maxHighScores)
                .ToList();

            foreach (var scoreToRemove in highScoresToRemove)
            {
                _highScores.Remove(scoreToRemove);
            }
        }
    }
}