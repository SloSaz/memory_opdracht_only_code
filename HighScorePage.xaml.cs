using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Business;
using Business.Models;
using DataAccess;

namespace MAUI.Pages;

public partial class HighScorePage : ContentPage
{
    private readonly IHighScoreRepository _highScoreRepository;

    public HighScorePage()
    {
        InitializeComponent();

        // Initialize high score repository
        string highScoreFilePath = Path.Combine(
            FileSystem.AppDataDirectory,
            "highscores.json");
        _highScoreRepository = new JsonHighScoreRepository(highScoreFilePath);

        LoadHighScores();
    }

    private void LoadHighScores()
    {
        List<HighScore> highScores = _highScoreRepository.GetTopHighScores(10);

        var highScoreViewModels = highScores
            .Select((score, index) => new HighScoreViewModel
            {
                Rank = index + 1,
                PlayerName = score.PlayerName,
                Score = score.Score,
                CardCount = score.CardCount,
                Attempts = score.Attempts,
                Time = $"{score.Duration.Minutes:00}:{score.Duration.Seconds:00}",
                Date = score.Date.ToShortDateString()
            })
            .ToList();

        HighScoresCollectionView.ItemsSource = highScoreViewModels;
    }

    private async void BackButton_Clicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("///WelcomePage");
    }
}

public class HighScoreViewModel
{
    public int Rank { get; set; }
    public string PlayerName { get; set; }
    public int Score { get; set; }
    public int CardCount { get; set; }
    public int Attempts { get; set; }
    public string Time { get; set; }
    public string Date { get; set; }
}