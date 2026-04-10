using TagAndTrack.Backend;
using TagAndTrack.Backend.Data;
using TagAndTrack.Backend.Utils;
using TagAndTrack.Components;

namespace TagAndTrack.Pages
{
    public class SettingsPage : TagAndTrackPage
    {
        protected const string titleText = "Settings";
        public SettingsPage() { Initialize(); }

        protected override void Initialize()
        {
            Background = CurrentTheme.Instance.Theme.Background;
            CurrentTheme.Instance.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(CurrentTheme.Theme))
                {
                    Background = CurrentTheme.Instance.Theme.Background;
                }
            };
            var header = new HeaderTemplate(titleText);

            var themeButton = new TagAndTrackButton("Light/Dark Mode", new Command(() => CurrentTheme.Instance.SwitchTheme()), "day_and_night.png");
            var exportButton = new TagAndTrackButton("Export Database (CSV)", new Command(async () => await ExportDatabaseAsync()), "document.png");
            var importButton = new TagAndTrackButton("Import Database (CSV)", new Command(async () => await ImportDatabaseAsync()), "enter.png");
            var clearDbButton = new TagAndTrackButton("Clear Database", new Command(async () => await ClearDatabaseAsync()), "trash.png");

            Content = new VerticalStackLayout()
            {
                Children =
                {
                    header,
                    themeButton,
                    exportButton,
                    importButton,
                    clearDbButton
                }
            };
        }

        private async Task ExportDatabaseAsync()
        {
            try
            {
                var exportDir = await CsvService.ExportDatabaseAsync();

                var files = new List<ShareFile>();
                foreach (var file in Directory.GetFiles(exportDir, "*.csv"))
                {
                    files.Add(new ShareFile(file));
                }

                await Share.Default.RequestAsync(new ShareMultipleFilesRequest
                {
                    Title = "Export Tag & Track Database",
                    Files = files
                });
            }
            catch (Exception ex)
            {
                DebugLogger.Log($"ExportDatabaseAsync ERROR: {ex.Message}");
                await DisplayAlert("Error", $"Export failed: {ex.Message}", "OK");
            }
        }

        private async Task ImportDatabaseAsync()
        {
            bool confirm = await DisplayAlert(
                "Import Database",
                "This will REPLACE all existing data with the imported data. This action cannot be undone. Continue?",
                "Import",
                "Cancel");

            if (!confirm) return;

            try
            {
                var csvTypes = new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
                {
                    { DevicePlatform.WinUI, new[] { ".csv" } },
                    { DevicePlatform.iOS, new[] { "public.comma-separated-values-text" } },
                    { DevicePlatform.macOS, new[] { "public.comma-separated-values-text" } },
                    { DevicePlatform.Android, new[] { "text/csv" } }
                });

                var results = await FilePicker.Default.PickMultipleAsync(new PickOptions
                {
                    PickerTitle = "Select CSV files (specimens, loans, containers, employees)",
                    FileTypes = csvTypes
                });

                if (results == null || !results.Any()) return;

                // Copy picked files to a temp folder so CsvService can read them by name
                var importDir = Path.Combine(FileSystem.CacheDirectory, "TagAndTrack_Import");
                if (Directory.Exists(importDir))
                    Directory.Delete(importDir, true);
                Directory.CreateDirectory(importDir);

                foreach (var result in results)
                {
                    var destPath = Path.Combine(importDir, result.FileName);
                    using var source = await result.OpenReadAsync();
                    using var dest = File.Create(destPath);
                    await source.CopyToAsync(dest);
                }

                await CsvService.ImportDatabaseAsync(importDir);
                await DisplayAlert("Done", "Database imported successfully.", "OK");
                await Shell.Current.GoToAsync("//LoginPage");
            }
            catch (Exception ex)
            {
                DebugLogger.Log($"ImportDatabaseAsync ERROR: {ex.Message}");
                await DisplayAlert("Error", $"Import failed: {ex.Message}", "OK");
            }
        }

        private async Task ClearDatabaseAsync()
        {
            bool confirm = await DisplayAlert(
                "Clear Database",
                "This will permanently delete ALL data (specimens, loans, containers, employees). This action cannot be undone.",
                "I understand, clear it",
                "Cancel");

            if (!confirm) return;

            try
            {
                await DbService.ResetDatabaseAsync();
                await DisplayAlert("Done", "Database has been cleared.", "OK");
                await Shell.Current.GoToAsync("//LoginPage");
            }
            catch (Exception ex)
            {
                DebugLogger.Log($"ClearDatabaseAsync ERROR: {ex.Message}");
                await DisplayAlert("Error", $"Clear failed: {ex.Message}", "OK");
            }
        }
    }
}