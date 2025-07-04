namespace Business.Models;

/// <summary>
/// Represents a high score entry
/// </summary>
public class HighScore(string playerName, int score, int cardCount, int attempts, TimeSpan duration)
{
    public int Id { get; set; }
    public string PlayerName { get; } = playerName ?? throw new ArgumentNullException(nameof(playerName));
    public int Score { get; } = score;
    public int CardCount { get; } = cardCount;
    public int Attempts { get; } = attempts;
    public TimeSpan Duration { get; } = duration;
    public DateTime Date { get; } = DateTime.Now;
}