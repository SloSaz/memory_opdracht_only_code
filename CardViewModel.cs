using System.ComponentModel;
using System.Runtime.CompilerServices;
using Business.Models;

namespace MAUI.ViewModels;

public class CardViewModel : INotifyPropertyChanged
{
    private readonly Card _card;

    public int Id => _card.Id;
    public string Symbol => _card.Symbol;

    private bool _isSymbolVisible;

    public bool IsSymbolVisible
    {
        get => _isSymbolVisible;
        set
        {
            if (_isSymbolVisible != value)
            {
                _isSymbolVisible = value;
                OnPropertyChanged();
            }
        }
    }
    
    private Color _backgroundColor;
    public Color BackgroundColor
    {
        get => _backgroundColor;
        set
        {
            if (_backgroundColor != value)
            {
                _backgroundColor = value;
                OnPropertyChanged();
            }
        }
    }
    
    private bool _canFlip;
    public bool CanFlip
    {
        get => _canFlip;
        set
        {
            if (_canFlip != value)
            {
                _canFlip = value;
                OnPropertyChanged();
            }
        }
    }
    
    public CardViewModel(Card card)
    {
        _card = card;
        UpdateState();
        UpdateCanFlip();
    }
    
    public void UpdateState()
    {
        IsSymbolVisible = _card.IsFaceUp;
            
        if (_card.IsFaceUp)
        {
            // Parse symbol to get index
            if (int.TryParse(_card.Symbol, out int symbolIndex))
            {
                // Get color based on symbol
                BackgroundColor = GetCardColor(symbolIndex - 1);
            }
            else
            {
                BackgroundColor = Colors.DarkBlue;
            }
        }
        else
        {
            BackgroundColor = Colors.DimGray;
        }
    }
        
    public void UpdateCanFlip()
    {
        CanFlip = !_card.IsFaceUp && !_card.IsMatched;
    }
    
    private Color GetCardColor(int index)
    {
        // Define a set of colors
        Color[] colors = 
        {
            Colors.Red,
            Colors.Blue,
            Colors.Green,
            Colors.Orange,
            Colors.Purple,
            Colors.Teal,
            Colors.Brown,
            Colors.Chocolate,
            Colors.ForestGreen,
            Colors.Indigo,
            Colors.Crimson,
            Colors.MediumVioletRed,
            Colors.DarkCyan,
            Colors.SaddleBrown,
            Colors.DarkSlateBlue
        };
            
        return colors[index % colors.Length];
    }
    
    public event PropertyChangedEventHandler PropertyChanged;
        
    protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}