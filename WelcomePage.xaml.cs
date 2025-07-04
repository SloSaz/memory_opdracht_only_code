using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MAUI.Pages;

public partial class WelcomePage : ContentPage
{
    public WelcomePage()
    {
        InitializeComponent();

        // Load saved player name if available
        LoadPlayerName();
    }

    private async void LoadPlayerName()
    {
        try
        {
            // Using Preferences API to store/retrieve simple values
            if (Preferences.ContainsKey("PlayerName"))
            {
                PlayerNameEntry.Text = Preferences.Get("PlayerName", string.Empty);
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Failed to load player name: {ex.Message}", "OK");
        }
    }
    
    private async void StartGameButton_Clicked(object sender, EventArgs e)
    {
        string playerName = PlayerNameEntry.Text?.Trim();
            
        if (string.IsNullOrEmpty(playerName))
        {
            await DisplayAlert("Input Required", "Please enter your name.", "OK");
            return;
        }
            
        // Save player name for next time
        Preferences.Set("PlayerName", playerName);
            
        // Get number of pairs
        int pairCount = (int)PairsSlider.Value;
            
        // Navigate to game page with parameters
        var navigationParameter = new Dictionary<string, object>
        {
            { "PlayerName", playerName },
            { "PairCount", pairCount }
        };
            
        await Shell.Current.GoToAsync($"{nameof(GamePage)}", navigationParameter);
    }
    
    private async void HighScoresButton_Clicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync(nameof(HighScorePage));
    }
        
    private void ExitButton_Clicked(object sender, EventArgs e)
    {
        Application.Current.Quit();
    }
}

/*
500+500+500+600+600+750+750+750+750+800+800+800+900+900+900+1.000+1.000+1.000+1.000+1.000+1.100+1.150+1.200+1.200+1.200+1.300+1.300+1.350+1.400+1.400+1.500+1.500+1.500+1.600+1.600+1.600+1.750+1.800+1.800+1.900+2.000+2.000+2.250+2.500+2.500+2.750+3.000+3.500+4.000+4.000
	
	 600   | 1.400 | 4.000 | 2.250 | 1.300 |
 
| 750   | 800   | 1.500 | 3.500 | 1.500 |
| 1.200 | 1.000 | 1.600 | 1.800 | 1.000 |
| 500   | 1.500 | 1.200 | 800   | 900   |
| 2.000 | 2.500 | 800   | 2.000 | 1.750 |
| 1.600 | 1.200 | 1.100 | 1.000 | 1.600 |
| 1.350 | 4.000 | 1.150 | 1.400 | 750   |
| 1.800 | 900   | 500   | 1.900 | 3.000 |
| 900   | 750   | 750   | 500   | 2.750 |
| 1.000 | 1.000 | 1.300 | 600   | 2.500 |
 */