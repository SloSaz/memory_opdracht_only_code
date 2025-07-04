namespace MAUI.Services;

public interface INavigationService
{
    Task NavigateToAsync<T>(object parameter = null) where T : Page;
    Task GoBackAsync();
}