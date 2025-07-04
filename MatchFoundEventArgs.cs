using Business.Models;

namespace Business.Events;
/// <summary>
/// Event arguments for when a match is found
/// </summary>
public class MatchFoundEventArgs(Card firstCard, Card secondCard) : EventArgs
{
    public Card FirstCard { get; } = firstCard;
    public Card SecondCard { get; } = secondCard;
}