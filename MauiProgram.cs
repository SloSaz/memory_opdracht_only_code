using Business;
using Business.Services;
using DataAccess;
using MAUI.Pages;
using MAUI.Services;
using MAUI.ViewModels;
using Microsoft.Extensions.Logging;
using HighScoreViewModel = MAUI.ViewModels.HighScoreViewModel;

namespace MAUI;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });
        
        ConfigureServices(builder.Services);

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }

    private static void ConfigureServices(IServiceCollection services)
    {
        string appDataDir = Path.Combine(FileSystem.AppDataDirectory, "Memory");
        if (!Directory.Exists(appDataDir))
        {
            Directory.CreateDirectory(appDataDir);
        }
        
        string highScoresFilePath = Path.Combine(appDataDir, "highscores.json");
        string customCardImagesFilePath = Path.Combine(appDataDir, "customcardimages.json");
        string customCardImagesDirectory = Path.Combine(appDataDir, "CardImages");


        services.AddSingleton<IHighScoreRepository>(sp => 
            new JsonHighScoreRepository(highScoresFilePath));

        services.AddSingleton<INavigationService, NavigationService>();
        services.AddTransient<MemoryGameService>();

        services.AddTransient<GameViewModel>();
        services.AddTransient<HighScoreViewModel>();

        services.AddTransient<GamePage>();
        services.AddTransient<HighScorePage>();
        
        services.AddSingleton<ICustomCardImageRepository>(sp => 
            new JsonCustomCardImageRepository(customCardImagesFilePath, customCardImagesDirectory));
        services.AddSingleton<CustomCardImageService>(sp => 
            new CustomCardImageService(
                sp.GetRequiredService<ICustomCardImageRepository>(),
                customCardImagesDirectory));
        
        services.AddSingleton<IFilePickerService, FilePickerService>();
        services.AddTransient<CustomCardImagesViewModel>();
        services.AddTransient<CustomCardImagesPage>();
    }
}