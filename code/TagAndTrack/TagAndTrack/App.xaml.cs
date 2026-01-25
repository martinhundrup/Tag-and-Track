using TagAndTrack.Backend;
using TagAndTrack.Backend.Data;

namespace TagAndTrack
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            // Initialize logger FIRST so we can track everything
            DebugLogger.Init();
            DebugLogger.Log("=== APP CONSTRUCTOR STARTED ===");

            try
            {
                // Initialize database synchronously at startup
                DebugLogger.Log("Starting database initialization...");
                Task.Run(async () =>
                {
                    try
                    {
                        DebugLogger.Log("DbService.InitAsync() starting...");
                        await DbService.InitAsync();
                        DebugLogger.Log("DbService.InitAsync() completed");

                        DebugLogger.Log("DbService.SeedIfEmptyAsync() starting...");
                        await DbService.SeedIfEmptyAsync();
                        DebugLogger.Log("DbService.SeedIfEmptyAsync() completed");
                    }
                    catch (Exception ex)
                    {
                        DebugLogger.Log($"DATABASE INIT ERROR: {ex.GetType().Name}: {ex.Message}");
                        DebugLogger.Log($"Stack trace: {ex.StackTrace}");
                    }
                }).GetAwaiter().GetResult();

                DebugLogger.Log("Database initialization finished");
            }
            catch (Exception ex)
            {
                DebugLogger.Log($"APP INIT ERROR: {ex.GetType().Name}: {ex.Message}");
                DebugLogger.Log($"Stack trace: {ex.StackTrace}");
            }

            DebugLogger.Log("Creating AppShell...");
            MainPage = new AppShell();
            DebugLogger.Log("AppShell created, App constructor complete");
        }
    }
}
