using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using Business.Models;
using Business.Services;
using MAUI.Commands;
using MAUI.Services;

namespace MAUI.ViewModels
{
    public class CustomCardImagesViewModel : INotifyPropertyChanged
    {
        private readonly CustomCardImageService _imageService;
        private readonly INavigationService _navigationService;
        private readonly IFilePickerService _filePickerService;

        public ObservableCollection<CustomCardImage> Images { get; } = new();

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

        private string _newImageName;
        public string NewImageName
        {
            get => _newImageName;
            set
            {
                if (_newImageName != value)
                {
                    _newImageName = value;
                    OnPropertyChanged();
                }
            }
        }

        public ICommand GoBackCommand { get; }
        public ICommand AddNewImageCommand { get; }
        public ICommand DeleteImageCommand { get; }

        public CustomCardImagesViewModel(
            CustomCardImageService imageService,
            INavigationService navigationService,
            IFilePickerService filePickerService)
        {
            _imageService = imageService ?? throw new ArgumentNullException(nameof(imageService));
            _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
            _filePickerService = filePickerService ?? throw new ArgumentNullException(nameof(filePickerService));

            GoBackCommand = new RelayCommand(async () => await _navigationService.GoBackAsync());
            AddNewImageCommand = new RelayCommand(async () => await AddNewImageAsync());
            DeleteImageCommand = new RelayCommand<string>(async (id) => await DeleteImageAsync(id));

            LoadImages();
        }

        private void LoadImages()
        {
            IsLoading = true;

            try
            {
                Images.Clear();
                var images = _imageService.GetAllImages();

                foreach (var image in images)
                {
                    Images.Add(image);
                }
            }
            catch (Exception ex)
            {
                Application.Current.MainPage.DisplayAlert("Error", $"Error loading images: {ex.Message}", "OK");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task AddNewImageAsync()
        {
            if (string.IsNullOrWhiteSpace(NewImageName))
            {
                await Application.Current.MainPage.DisplayAlert("Error", "Please enter an image name", "OK");
                return;
            }

            try
            {
                IsLoading = true;

                // Pick an image file
                var result = await _filePickerService.PickImageAsync();
                if (result != null)
                {
                    // Add the image
                    using (var stream = await result.OpenReadAsync())
                    {
                        string fileExtension = Path.GetExtension(result.FileName);
                        var image = await _imageService.AddImageAsync(NewImageName, stream, fileExtension);
                        Images.Add(image);
                    }

                    // Clear the name field
                    NewImageName = string.Empty;
                }
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Error", $"Error adding image: {ex.Message}", "OK");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task DeleteImageAsync(string id)
        {
            if (string.IsNullOrEmpty(id))
                return;

            var confirm = await Application.Current.MainPage.DisplayAlert(
                "Confirm Delete",
                "Are you sure you want to delete this image?",
                "Yes", "No");

            if (confirm)
            {
                try
                {
                    IsLoading = true;

                    // Delete the image
                    _imageService.DeleteImage(id);

                    // Remove from collection
                    var image = Images.FirstOrDefault(i => i.Id == id);
                    if (image != null)
                    {
                        Images.Remove(image);
                    }
                }
                catch (Exception ex)
                {
                    await Application.Current.MainPage.DisplayAlert("Error", $"Error deleting image: {ex.Message}", "OK");
                }
                finally
                {
                    IsLoading = false;
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}