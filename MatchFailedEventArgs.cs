using Business.Models;

namespace Business.Events;


/// <summary>
/// Event arguments for when a match attempt fails
/// </summary>
public class MatchFailedEventArgs(Card firstCard, Card secondCard) : EventArgs
{
    public Card FirstCard { get; } = firstCard;
    public Card SecondCard { get; } = secondCard;
}