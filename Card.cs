namespace Business.Models;

/// <summary>
/// Represents a memory card in the game
/// </summary>
public class Card(int id, string symbol)
{
    public int Id { get; } = id;
    public string Symbol { get; } = symbol;
    public bool IsFaceUp { get; private set; } = false;
    public bool IsMatched { get; private set; } = false;

    public void Flip()
    {
        IsFaceUp = !IsFaceUp;
    }

    public void SetMatched()
    {
        IsMatched = true;
        IsFaceUp = true;
    }

    public void Reset()
    {
        IsFaceUp = false;
        IsMatched = false;
    }
}