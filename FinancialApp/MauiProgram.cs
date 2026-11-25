using Microsoft.Extensions.Logging;
using CommunityToolkit.Maui;

namespace FinancialApp
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .UseMauiCommunityToolkit()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });
            builder.ConfigureFonts(fonts =>
            {
                fonts.AddFont("Raleway-Regular.ttf", "RalewayRegular");
                fonts.AddFont("Raleway-Medium.ttf", "RalewayMedium");
                fonts.AddFont("Raleway-SemiBold.ttf", "RalewaySemiBold");
                fonts.AddFont("Raleway-Bold.ttf", "RalewayBold");
            });
#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
