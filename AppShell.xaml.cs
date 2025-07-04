using MAUI.Pages;

namespace MAUI;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();
        
        Routing.RegisterRoute(nameof(GamePage), typeof(GamePage));
        Routing.RegisterRoute(nameof(HighScorePage), typeof(HighScorePage));
    }
}