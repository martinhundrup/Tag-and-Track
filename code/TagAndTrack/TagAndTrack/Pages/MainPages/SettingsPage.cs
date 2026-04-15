using System.ComponentModel;
using TagAndTrack.Backend;
using TagAndTrack.Backend.Data;
using TagAndTrack.Backend.Utils;
using TagAndTrack.Components;

namespace TagAndTrack.Pages
{
    public class SettingsPage : TagAndTrackPage, IDisposable
    {
        private PropertyChangedEventHandler handler;
        protected const string titleText = "Settings";
        public SettingsPage() { Initialize(); }

        protected override void Initialize()
        {
            Background = CurrentTheme.Instance.Theme.Background;
            handler = (s, e) =>
            {
                if (e.PropertyName == nameof(CurrentTheme.Theme))
                {
                    Background = CurrentTheme.Instance.Theme.Background;
                }
            };
            CurrentTheme.Instance.PropertyChanged += handler;
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
                var exportFilePath = await CsvService.ExportDatabaseAsync();

#if WINDOWS
                // Upon Windows: beseech the user to choose a place of saving
                var savePicker = new Windows.Storage.Pickers.FileSavePicker();
                savePicker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.DocumentsLibrary;
                savePicker.FileTypeChoices.Add("CSV File", new List<string> { ".csv" });
                savePicker.SuggestedFileName = "tagandtrack_export";

                var hwnd = ((MauiWinUIWindow)App.Current!.Windows[0].Handler!.PlatformView!).WindowHandle;
                WinRT.Interop.InitializeWithWindow.Initialize(savePicker, hwnd);

                var file = await savePicker.PickSaveFileAsync();
                if (file == null) return;

                File.Copy(exportFilePath, file.Path, overwrite: true);
                await DisplayAlert("Done", $"CSV saved to:\n{file.Path}", "OK");
#else
                // Upon iOS or Mac: employ the Share sheet, which doth offer Save to Files
                await Share.Default.RequestAsync(new ShareFileRequest
                {
                    Title = "Export Tag & Track Database",
                    File = new ShareFile(exportFilePath)
                });
#endif
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

                var result = await FilePicker.Default.PickAsync(new PickOptions
                {
                    PickerTitle = "Select Tag & Track CSV export file",
                    FileTypes = csvTypes
                });

                if (result == null) return;

                // Copy the chosen file to a temporary dwelling, that CsvService might read it
                var importPath = Path.Combine(FileSystem.CacheDirectory, "tagandtrack_import.csv");
                using (var source = await result.OpenReadAsync())
                using (var dest = File.Create(importPath))
                {
                    await source.CopyToAsync(dest);
                }

                await CsvService.ImportDatabaseAsync(importPath);
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

        protected override void OnParentChanged()
        {
            base.OnParentChanged();
            if(Parent == null)
            {
                Dispose();
            }
        }

        public void Dispose()
        {
            CurrentTheme.Instance.PropertyChanged -= handler;
        }
    }
}