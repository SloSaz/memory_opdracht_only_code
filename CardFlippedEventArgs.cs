using Business.Models;

namespace Business.Events;

/// <summary>
/// Event arguments for when a card is flipped
/// </summary>
public class CardFlippedEventArgs(Card card) : EventArgs
{
    public Card Card { get; } = card;
}