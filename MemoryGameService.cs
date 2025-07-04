using Business.Events;
using Business.Exceptions;
using Business.Models;

namespace Business.Services;

public class MemoryGameService
{
    private readonly List<Card> _cards;
    private readonly Random _random;
    private readonly Player _player;
    private readonly IHighScoreRepository _highScoreRepository;
    private readonly DateTime _gameStartTime;

    private Card _firstSelectedCard;
    private int _attempts;
    private bool _gameCompleted;

    public IReadOnlyList<Card> Cards => _cards.AsReadOnly();
    public int CardCount => _cards.Count;
    public int RemainingPairs => _cards.Count(c => !c.IsMatched) / 2;
    public int Attempts => _attempts;
    public TimeSpan Duration => DateTime.Now - _gameStartTime;

    // Events
    public event EventHandler<CardFlippedEventArgs> CardFlipped;
    public event EventHandler<MatchFoundEventArgs> MatchFound;
    public event EventHandler<MatchFailedEventArgs> MatchFailed;
    public event EventHandler<GameCompletedEventArgs> GameCompleted;

    public MemoryGameService(Player player, int pairCount, IHighScoreRepository highScoreRepository)
    {
        if (pairCount < 2)
            throw new ArgumentException("Game must have at least 2 pairs", nameof(pairCount));

        _player = player ?? throw new ArgumentNullException(nameof(player));
        _highScoreRepository = highScoreRepository ?? throw new ArgumentNullException(nameof(highScoreRepository));
        _random = new Random();
        _cards = new List<Card>();
        _gameStartTime = DateTime.Now;
        _attempts = 0;
        _gameCompleted = false;

        // Initialize cards
        InitializeCards(pairCount);
    }

    private void InitializeCards(int pairCount)
    {
        _cards.Clear();

        // Create pairs of cards
        for (int i = 0; i < pairCount; i++)
        {
            string symbol = (i + 1).ToString();

            // Add two cards with the same symbol
            _cards.Add(new Card(i * 2, symbol));
            _cards.Add(new Card(i * 2 + 1, symbol));
        }

        // Shuffle cards
        ShuffleCards();
    }

    private void ShuffleCards()
    {
        for (int i = _cards.Count - 1; i > 0; i--)
        {
            int j = _random.Next(i + 1);
            Card temp = _cards[i];
            _cards[i] = _cards[j];
            _cards[j] = temp;
        }
    }

    public void FlipCard(int cardIndex)
    {
        if (_gameCompleted)
            throw new InvalidGameOperationException("Game is already completed");

        if (cardIndex < 0 || cardIndex >= _cards.Count)
            throw new ArgumentOutOfRangeException(nameof(cardIndex), "Card index is out of range");

        Card card = _cards[cardIndex];

        if (card.IsMatched || card.IsFaceUp)
            throw new InvalidGameOperationException("Cannot flip this card");

        // Flip the card
        card.Flip();
        OnCardFlipped(card);

        // Check for match
        if (_firstSelectedCard == null)
        {
            // This is the first card in a pair
            _firstSelectedCard = card;
        }
        else
        {
            // This is the second card
            _attempts++;

            // Check if cards match
            if (_firstSelectedCard.Symbol == card.Symbol)
            {
                // Match found
                _firstSelectedCard.SetMatched();
                card.SetMatched();
                OnMatchFound(_firstSelectedCard, card);
                _firstSelectedCard = null;

                // Check if game is completed
                if (RemainingPairs == 0)
                {
                    CompleteGame();
                }
            }
            else
            {
                // No match
                OnMatchFailed(_firstSelectedCard, card);

                // Cards will be flipped back after a delay
                // This would be handled by the UI timer
                _firstSelectedCard = null;
            }
        }
    }

    public void FlipCardBack(int cardIndex)
    {
        if (cardIndex < 0 || cardIndex >= _cards.Count)
            throw new ArgumentOutOfRangeException(nameof(cardIndex), "Card index is out of range");

        Card card = _cards[cardIndex];

        if (!card.IsMatched && card.IsFaceUp)
            card.Flip();
    }

    private void CompleteGame()
    {
        _gameCompleted = true;

        // Calculate score
        int score = CalculateScore();

        // Check if it's a high score
        bool isHighScore = _highScoreRepository.IsHighScore(score);

        // Create game completed event args
        var args = new GameCompletedEventArgs(_player, score, CardCount, Attempts, Duration)
        {
            IsHighScore = isHighScore
        };

        // Save high score if applicable
        if (isHighScore)
        {
            var highScore = new HighScore(_player.Name, score, CardCount, Attempts, Duration);
            _highScoreRepository.SaveHighScore(highScore);
        }

        // Raise event
        OnGameCompleted(args);
    }

    private int CalculateScore()
    {
        // Score formula: ((Number of cards)² / (Time in seconds * number of attempts)) * 1000
        double score = Math.Pow(CardCount, 2) / (Duration.TotalSeconds * Attempts) * 1000;

        // Round to nearest integer
        return (int)Math.Round(score);
    }

    protected virtual void OnCardFlipped(Card card)
    {
        CardFlipped?.Invoke(this, new CardFlippedEventArgs(card));
    }

    protected virtual void OnMatchFound(Card firstCard, Card secondCard)
    {
        MatchFound?.Invoke(this, new MatchFoundEventArgs(firstCard, secondCard));
    }

    protected virtual void OnMatchFailed(Card firstCard, Card secondCard)
    {
        MatchFailed?.Invoke(this, new MatchFailedEventArgs(firstCard, secondCard));
    }

    protected virtual void OnGameCompleted(GameCompletedEventArgs args)
    {
        GameCompleted?.Invoke(this, args);
    }
}