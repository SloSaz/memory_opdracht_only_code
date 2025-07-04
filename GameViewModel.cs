using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Business.Events;
using Business.Models;
using Business.Services;
using MAUI.Commands;
using MAUI.Services;

namespace MAUI.ViewModels;

public class GameViewModel : INotifyPropertyChanged
{
    private readonly MemoryGameService _gameService;
    private readonly INavigationService _navigationService;
    private readonly System.Timers.Timer _matchFailedTimer;
    private readonly int _flipBackDelay = 1000; // 1 second

    public ObservableCollection<CardViewModel> Cards { get; } = new();

    private int _attempts;

    public int Attempts
    {
        get => _attempts;
        set
        {
            if (_attempts != value)
            {
                _attempts = value;
                OnPropertyChanged();
            }
        }
    }

    private int _remainingPairs;

    public int RemainingPairs
    {
        get => _remainingPairs;
        set
        {
            if (_remainingPairs != value)
            {
                _remainingPairs = value;
                OnPropertyChanged();
            }
        }
    }

    private TimeSpan _elapsedTime;

    public TimeSpan ElapsedTime
    {
        get => _elapsedTime;
        set
        {
            if (_elapsedTime != value)
            {
                _elapsedTime = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(ElapsedTimeDisplay));
            }
        }
    }

    public string ElapsedTimeDisplay => $"{ElapsedTime.Minutes:00}:{ElapsedTime.Seconds:00}";

    private bool _isGameCompleted;

    public bool IsGameCompleted
    {
        get => _isGameCompleted;
        set
        {
            if (_isGameCompleted != value)
            {
                _isGameCompleted = value;
                OnPropertyChanged();
            }
        }
    }

    private string _playerName;

    public string PlayerName
    {
        get => _playerName;
        set
        {
            if (_playerName != value)
            {
                _playerName = value;
                OnPropertyChanged();
            }
        }
    }

    private int _cardPairCount = 5;

    public int CardPairCount
    {
        get => _cardPairCount;
        set
        {
            if (_cardPairCount != value && value >= 2 && value <= 15)
            {
                _cardPairCount = value;
                OnPropertyChanged();
            }
        }
    }

    public ICommand FlipCardCommand { get; }
    public ICommand StartNewGameCommand { get; }
    public ICommand NavigateToHighScoresCommand { get; }

    private readonly System.Timers.Timer _gameTimer;

    public GameViewModel(MemoryGameService gameService, INavigationService navigationService)
    {
        _gameService = gameService ?? throw new ArgumentNullException(nameof(gameService));
        _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));

        // Initialize commands
        FlipCardCommand = new RelayCommand<int>(ExecuteFlipCard, CanFlipCard);
        StartNewGameCommand = new RelayCommand(StartNewGame);
        NavigateToHighScoresCommand =
            new RelayCommand(async () => await _navigationService.NavigateToAsync<Pages.HighScorePage>());

        // Setup match failed timer
        _matchFailedTimer = new System.Timers.Timer(_flipBackDelay);
        _matchFailedTimer.Elapsed += OnMatchFailedTimerElapsed;
        _matchFailedTimer.AutoReset = false;

        // Setup game timer
        _gameTimer = new System.Timers.Timer(1000); // 1 second interval
        _gameTimer.Elapsed += OnGameTimerElapsed;
        _gameTimer.AutoReset = true;

        // Subscribe to game events
        SubscribeToGameEvents();

        // Initialize with default values
        PlayerName = "Player 1";
    }

    private void SubscribeToGameEvents()
    {
        _gameService.CardFlipped += OnCardFlipped;
        _gameService.MatchFound += OnMatchFound;
        _gameService.MatchFailed += OnMatchFailed;
        _gameService.GameCompleted += OnGameCompleted;
    }

    private void UnsubscribeFromGameEvents()
    {
        _gameService.CardFlipped -= OnCardFlipped;
        _gameService.MatchFound -= OnMatchFound;
        _gameService.MatchFailed -= OnMatchFailed;
        _gameService.GameCompleted -= OnGameCompleted;
    }

    private void OnCardFlipped(object sender, CardFlippedEventArgs e)
    {
        UpdateCardViewModel(e.Card);
    }

    private void OnMatchFound(object sender, MatchFoundEventArgs e)
    {
        UpdateCardViewModel(e.FirstCard);
        UpdateCardViewModel(e.SecondCard);
        RemainingPairs = _gameService.RemainingPairs;
        Attempts = _gameService.Attempts;
    }

    private void OnMatchFailed(object sender, MatchFailedEventArgs e)
    {
        UpdateCardViewModel(e.FirstCard);
        UpdateCardViewModel(e.SecondCard);
        Attempts = _gameService.Attempts;

        // Start timer to flip cards back
        _matchFailedTimer.Start();
    }

    private void OnMatchFailedTimerElapsed(object sender, System.Timers.ElapsedEventArgs e)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            // Find cards that need to be flipped back
            foreach (var card in _gameService.Cards)
            {
                if (!card.IsMatched && card.IsFaceUp)
                {
                    _gameService.FlipCardBack(card.Id);
                    UpdateCardViewModel(card);
                }
            }
        });
    }

    private void OnGameCompleted(object sender, GameCompletedEventArgs e)
    {
        IsGameCompleted = true;
        _gameTimer.Stop();

        MainThread.BeginInvokeOnMainThread(async () =>
        {
            string message =
                $"Game completed!\nScore: {e.Score}\nTime: {e.Duration.Minutes:00}:{e.Duration.Seconds:00}\nAttempts: {e.Attempts}";

            if (e.IsHighScore)
            {
                message += "\n\nCongratulations! You got a high score!";
            }

            await Application.Current.MainPage.DisplayAlert("Game Over", message, "OK");

            if (e.IsHighScore)
            {
                await _navigationService.NavigateToAsync<Pages.HighScorePage>();
            }
        });
    }

    private void OnGameTimerElapsed(object sender, System.Timers.ElapsedEventArgs e)
    {
        MainThread.BeginInvokeOnMainThread(() => { ElapsedTime = ElapsedTime.Add(TimeSpan.FromSeconds(1)); });
    }

    private void UpdateCardViewModel(Card card)
    {
        var cardViewModel = Cards.FirstOrDefault(c => c.Id == card.Id);
        if (cardViewModel != null)
        {
            cardViewModel.UpdateState();
            cardViewModel.UpdateCanFlip();
        }
    }

    private bool CanFlipCard(int cardId)
    {
        if (IsGameCompleted || _matchFailedTimer.Enabled)
            return false;

        var card = _gameService.Cards.FirstOrDefault(c => c.Id == cardId);
        return card != null && !card.IsFaceUp && !card.IsMatched;
    }

    private void ExecuteFlipCard(int cardId)
    {
        try
        {
            int index = _gameService.Cards.FindIndex(c => c.Id == cardId);
            if (index >= 0)
            {
                _gameService.FlipCard(index);
            }
        }
        catch (Exception ex)
        {
            Application.Current.MainPage.DisplayAlert("Error", $"Error flipping card: {ex.Message}", "OK");
        }
    }

    public void StartNewGame()
    {
        // Stop timers
        _matchFailedTimer.Stop();
        _gameTimer.Stop();

        // Unsubscribe from old game events
        UnsubscribeFromGameEvents();

        // Create new player and game
        var player = new Player(PlayerName);
        var gameService = new MemoryGameService(player, CardPairCount, ((App)Application.Current).HighScoreRepository);
        _gameService = gameService;

        // Subscribe to new game events
        SubscribeToGameEvents();

        // Reset properties
        IsGameCompleted = false;
        Attempts = 0;
        ElapsedTime = TimeSpan.Zero;
        RemainingPairs = _gameService.RemainingPairs;

        // Update cards collection
        Cards.Clear();
        foreach (var card in _gameService.Cards)
        {
            Cards.Add(new CardViewModel(card));
        }

        // Start game timer
        _gameTimer.Start();
    }

    public event PropertyChangedEventHandler PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}


}