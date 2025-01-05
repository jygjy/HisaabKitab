using HisaabKitab.Services;
using Microsoft.Extensions.Logging;

namespace HisaabKitab
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();

            // Register the main app and fonts for usage
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                });

            // Register your custom UserService as a singleton
            builder.Services.AddSingleton<UserService>();
            builder.Services.AddSingleton<CashFlowService>();


            // Add the Blazor WebView service, enabling Blazor components to be rendered
            builder.Services.AddMauiBlazorWebView();

            // Add developer tools and debug logging for Blazor WebView when in DEBUG mode
#if DEBUG
            builder.Services.AddBlazorWebViewDeveloperTools();
            builder.Logging.AddDebug();
#endif

            // Return the built MauiApp
            return builder.Build();
        }
    }
}
