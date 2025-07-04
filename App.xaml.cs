using Business;

namespace MAUI;

public partial class App : Application
{
    // Expose the repository for easier access in the application
    public IHighScoreRepository HighScoreRepository { get; }

    public App(IHighScoreRepository highScoreRepository)
    {
        InitializeComponent();

        HighScoreRepository = highScoreRepository;
        // Configure global exception handling
        AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;
        TaskScheduler.UnobservedTaskException += OnUnobservedTaskException;

        MainPage = new AppShell();
    }

    private void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        var exception = e.ExceptionObject as Exception;
        LogException(exception, "Unhandled Exception");
    }

    private void OnUnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
    {
        LogException(e.Exception, "Unobserved Task Exception");
        e.SetObserved(); // Prevent the application from crashing
    }

    private void LogException(Exception exception, string type)
    {
        if (exception == null) return;

#if DEBUG
        System.Diagnostics.Debug.WriteLine($"{type}: {exception.Message}");
        System.Diagnostics.Debug.WriteLine(exception.StackTrace);
#endif

        MainThread.BeginInvokeOnMainThread(async () =>
        {
            await MainPage.DisplayAlert("Error",
                "An unexpected error occurred. Please try again or restart the application.",
                "OK");
        });
    }

    protected override void OnStart()
    {
        base.OnStart();
    }

    protected override void OnSleep()
    {
        base.OnSleep();
    }

    protected override void OnResume()
    {
        base.OnResume();
    }
}