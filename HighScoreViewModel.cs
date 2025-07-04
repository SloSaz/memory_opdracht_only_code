using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Business;
using Business.Models;
using MAUI.Commands;
using MAUI.Services;

namespace MAUI.ViewModels
{
    public class HighScoreViewModel : INotifyPropertyChanged
    {
        private readonly IHighScoreRepository _highScoreRepository;
        private readonly INavigationService _navigationService;
        
        public ObservableCollection<HighScore> HighScores { get; } = new();
        
        private bool _isLoading;
        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                if (_isLoading != value)
                {
                    _isLoading = value;
                    OnPropertyChanged();
                }
            }
        }
        
        public ICommand GoBackCommand { get; }
        public ICommand StartNewGameCommand { get; }
        
        public HighScoreViewModel(IHighScoreRepository highScoreRepository, INavigationService navigationService)
        {
            _highScoreRepository = highScoreRepository ?? throw new ArgumentNullException(nameof(highScoreRepository));
            _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
            
            GoBackCommand = new RelayCommand(async () => await _navigationService.GoBackAsync());
            StartNewGameCommand = new RelayCommand(async () => await _navigationService.NavigateToAsync<Pages.GamePage>());
            
            LoadHighScores();
        }
        
        public void LoadHighScores()
        {
            IsLoading = true;
            
            try
            {
                HighScores.Clear();
                var scores = _highScoreRepository.GetTopHighScores(10);
                
                foreach (var score in scores)
                {
                    HighScores.Add(score);
                }
            }
            catch (Exception ex)
            {
                Application.Current.MainPage.DisplayAlert("Error", $"Error loading high scores: {ex.Message}", "OK");
            }
            finally
            {
                IsLoading = false;
            }
        }
        
        public event PropertyChangedEventHandler PropertyChanged;
        
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}