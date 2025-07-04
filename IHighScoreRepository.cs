using Business.Models;

namespace Business;


/// <summary>
/// Repository interface for high score data access
/// </summary>
public interface IHighScoreRepository
{
    List<HighScore> GetTopHighScores(int count);
    void SaveHighScore(HighScore highScore);
    bool IsHighScore(int score);
}