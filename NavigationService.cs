namespace MAUI.Services
{
    public class NavigationService : INavigationService
    {
        protected INavigation Navigation => Application.Current.MainPage.Navigation;

        public async Task NavigateToAsync<T>(object parameter = null) where T : Page
        {
            var page = Activator.CreateInstance<T>();
            
            // Set binding context if parameter is a view model
            if (parameter != null)
            {
                page.BindingContext = parameter;
            }
            
            await Navigation.PushAsync(page);
        }

        public async Task GoBackAsync()
        {
            await Navigation.PopAsync();
        }
    }
}