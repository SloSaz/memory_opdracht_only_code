using Business.Models;

namespace Business.Events;

/// <summary>
/// Event arguments for when a game is completed
/// </summary>
public class GameCompletedEventArgs(Player player, int score, int cardCount, int attempts, TimeSpan duration)
    : EventArgs
{
    public Player Player { get; } = player;
    public int Score { get; } = score;
    public int CardCount { get; } = cardCount;
    public int Attempts { get; } = attempts;
    public TimeSpan Duration { get; } = duration;
    public bool IsHighScore { get; set; } = false;
}