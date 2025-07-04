using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Business;
using Business.Events;
using Business.Exceptions;
using Business.Models;
using Business.Services;
using DataAccess;
using MAUI.ViewModels;

namespace MAUI.Pages;

[QueryProperty(nameof(PlayerName), "PlayerName")]
[QueryProperty(nameof(PairCount), "PairCount")]
public partial class GamePage : ContentPage, INotifyPropertyChanged
{
    private MemoryGameService _game;
    private readonly IHighScoreRepository _highScoreRepository;
    private IDispatcherTimer _gameTimer;
    private IDispatcherTimer _flipBackTimer;
    private Card _lastFlippedCard;
    private ObservableCollection<CardViewModel> _cards;

    #region Bindable Properties

    private string _playerName;
    public string PlayerName
    {
        get => _playerName;
        set
        {
            _playerName = value;
            OnPropertyChanged();
            // Initialize game when both properties are set
            InitializeGameIfReady();
        }
    }

    private int _pairCount;
    public int PairCount
    {
        get => _pairCount;
        set
        {
            _pairCount = value;
            OnPropertyChanged();
            // Initialize game when both properties are set
            InitializeGameIfReady();
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

    private int _totalPairs;
    public int TotalPairs
    {
        get => _totalPairs;
        set
        {
            if (_totalPairs != value)
            {
                _totalPairs = value;
                OnPropertyChanged();
            }
        }
    }

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

    private string _gameTime;
    public string GameTime
    {
        get => _gameTime;
        set
        {
            if (_gameTime != value)
            {
                _gameTime = value;
                OnPropertyChanged();
            }
        }
    }
    #endregion

    private bool _isGameInitialized = false;
    
    public GamePage()
    {
        InitializeComponent();
        BindingContext = this;
        
        // Initialize high score repository
        string highScoreFilePath = Path.Combine(
                FileSystem.AppDataDirectory,
                "highscores.json");
        _highScoreRepository = new JsonHighScoreRepository(highScoreFilePath);

        // Initialize timers
        _gameTimer = Dispatcher.CreateTimer();
        _gameTimer.Interval = TimeSpan.FromSeconds(1);
        _gameTimer.Tick += GameTimer_Tick;
        
        _flipBackTimer = Dispatcher.CreateTimer();
        _flipBackTimer.Interval = TimeSpan.FromMilliseconds(1000);
        _flipBackTimer.Tick += FlipBackTimer_Tick;
        
        // Initialize default values
        GameTime = "00:00";
        Attempts = 0;
    }
    
    private void InitializeGameIfReady()
    {
        // Check if both properties are set and game is not already initialized
        if (!string.IsNullOrEmpty(PlayerName) && PairCount > 0 && !_isGameInitialized)
        {
            _isGameInitialized = true;
                
            // Initialize game
            Player player = new Player(PlayerName);
            _game = new MemoryGameService(player, PairCount, _highScoreRepository);
                
            // Subscribe to game events
            _game.CardFlipped += Game_CardFlipped;
            _game.MatchFound += Game_MatchFound;
            _game.MatchFailed += Game_MatchFailed;
            _game.GameCompleted += Game_GameCompleted;
                
            // Initialize game stats
            RemainingPairs = _game.RemainingPairs;
            TotalPairs = PairCount;
            Attempts = 0;
                
            // Start game timer
            _gameTimer.Start();
                
            // Initialize cards
            InitializeCards();
        }
    }
    
    private void InitializeCards()
    {
        // Clear any existing cards
        CardsLayout.Clear();
            
        // Create and add card views
        foreach (var card in _game.Cards)
        {
            var cardViewModel = new CardViewModel(card);
            var cardView = CreateCardView(cardViewModel);
            CardsLayout.Add(cardView);
        }
    }
    
    private View CreateCardView(CardViewModel cardViewModel)
    {
        // Create frame for card
        var frame = new Frame
        {
            BackgroundColor = cardViewModel.BackgroundColor,
            BorderColor = Colors.Black,
            CornerRadius = 5,
            Margin = new Thickness(5),
            HeightRequest = 100,
            WidthRequest = 80,
            Padding = new Thickness(0)
        };
            
        // Create grid for card content
        var grid = new Grid();
            
        // Create label for card symbol
        var symbolLabel = new Label
        {
            Text = cardViewModel.Symbol,
            TextColor = Colors.White,
            FontSize = 36,
            FontAttributes = FontAttributes.Bold,
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.Center
        };
            
        // Set binding for visibility
        symbolLabel.SetBinding(IsVisibleProperty, new Binding
        {
            Source = cardViewModel,
            Path = nameof(CardViewModel.IsSymbolVisible),
            Mode = BindingMode.OneWay
        });
            
        // Add label to grid
        grid.Add(symbolLabel);
            
        // Add grid to frame
        frame.Content = grid;
            
        // Set tag to card view model
        frame.BindingContext = cardViewModel;
            
        // Add tap gesture
        var tapGesture = new TapGestureRecognizer();
        tapGesture.Tapped += CardTapped;
        frame.GestureRecognizers.Add(tapGesture);
            
        return frame;
    }
    
    private void CardTapped(object sender, EventArgs e)
    {
        var frame = (Frame)sender;
        var cardViewModel = (CardViewModel)frame.BindingContext;
            
        if (!cardViewModel.CanFlip)
            return;
            
        // Find index of card in the game
        int cardIndex = -1;
        for (int i = 0; i < _game.Cards.Count; i++)
        {
            if (_game.Cards[i].Id == cardViewModel.Id)
            {
                cardIndex = i;
                break;
            }
        }
            
        if (cardIndex >= 0)
        {
            try
            {
                _game.FlipCard(cardIndex);
            }
            catch (InvalidGameOperationException ex)
            {
                DisplayAlert("Error", ex.Message, "OK");
            }
        }
    }
    
    private void GameTimer_Tick(object sender, EventArgs e)
    {
        GameTime = $"{_game.Duration.Minutes:00}:{_game.Duration.Seconds:00}";
    }

    private void FlipBackTimer_Tick(object sender, EventArgs e)
    {
        _flipBackTimer.Stop();

        // Flip unmatched cards back
        for (int i = 0; i < _game.Cards.Count; i++)
        {
            var card = _game.Cards[i];
            if (card.IsFaceUp && !card.IsMatched)
            {
                _game.FlipCardBack(i);
            }
        }


        // Update all card view models
        foreach (var child in CardsLayout.Children)
        {
            var frame = child as Frame;
            var cardViewModel = frame?.BindingContext as CardViewModel;
            cardViewModel?.UpdateState();
            cardViewModel?.UpdateCanFlip();
        }
    }

    #region Game Event Handlers

    private void Game_CardFlipped(object sender, CardFlippedEventArgs e)
    {
        // Update the flipped card
        foreach (var child in CardsLayout.Children)
        {
            var frame = child as Frame;
            var cardViewModel = frame?.BindingContext as CardViewModel;
                
            if (cardViewModel != null && cardViewModel.Id == e.Card.Id)
            {
                cardViewModel.UpdateState();
                    
                // Update frame background color
                frame.BackgroundColor = cardViewModel.BackgroundColor;
                break;
            }
        }
            
        // If this is the second card, temporarily disable all cards
        if (_lastFlippedCard != null)
        {
            foreach (var child in CardsLayout.Children)
            {
                var frame = child as Frame;
                var cardViewModel = frame?.BindingContext as CardViewModel;
                if (cardViewModel != null)
                {
                    cardViewModel.CanFlip = false;
                }
            }
        }
        else
        {
            _lastFlippedCard = e.Card;
        }
    }
    
    private void Game_MatchFound(object sender, MatchFoundEventArgs e)
    {
        RemainingPairs = _game.RemainingPairs;
        Attempts = _game.Attempts;
        _lastFlippedCard = null;
            
        // Update all card view models
        foreach (var child in CardsLayout.Children)
        {
            var frame = child as Frame;
            var cardViewModel = frame?.BindingContext as CardViewModel;
                
            if (cardViewModel != null)
            {
                cardViewModel.UpdateState();
                cardViewModel.UpdateCanFlip();
                    
                // Update frame background color
                frame.BackgroundColor = cardViewModel.BackgroundColor;
            }
        }
    }
        
    private void Game_MatchFailed(object sender, MatchFailedEventArgs e)
    {
        Attempts = _game.Attempts;
        _lastFlippedCard = null;
            
        // Start timer to flip cards back after a delay
        _flipBackTimer.Start();
    }
        
    private async void Game_GameCompleted(object sender, GameCompletedEventArgs e)
    {
        _gameTimer.Stop();
            
        // Show game completed message
        string message = $"Congratulations! You've completed the game!\n\n" +
                         $"Score: {e.Score}\n" +
                         $"Time: {e.Duration.Minutes:00}:{e.Duration.Seconds:00}\n" +
                         $"Attempts: {e.Attempts}\n";
            
        if (e.IsHighScore)
        {
            message += "\nNew High Score!";
        }
            
        await DisplayAlert("Game Completed", message, "OK");
            
        // Navigate to high score page
        await Shell.Current.GoToAsync($"///{nameof(HighScorePage)}");
    }

    #endregion

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
            
        // Stop timers
        _gameTimer?.Stop();
        _flipBackTimer?.Stop();
    }
    
    
}