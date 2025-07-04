using MAUI.ViewModels;

namespace MAUI.Pages
{
    public partial class CustomCardImagesPage : ContentPage
    {
        private readonly CustomCardImagesViewModel _viewModel;

        public CustomCardImagesPage(CustomCardImagesViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            BindingContext = _viewModel;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            // Refresh could be added here if needed
        }
    }
}