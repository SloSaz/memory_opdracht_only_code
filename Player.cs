namespace Business.Models;


/// <summary>
/// Represents a player in the game
/// </summary>
public class Player
{
    public string Name { get; }

    public Player(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Player name cannot be empty", nameof(name));

        Name = name;
    }
}