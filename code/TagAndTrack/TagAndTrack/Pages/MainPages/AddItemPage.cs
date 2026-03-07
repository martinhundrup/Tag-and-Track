using System.Runtime.CompilerServices;
using TagAndTrack.Backend.Data.Entities;
using TagAndTrack.Components;
using TagAndTrack.Backend.Data;
using TagAndTrack.Backend.Items;

namespace TagAndTrack.Pages
{

    public class AddItemPage : TagAndTrackPage
    {
        protected const string titleText = "Add Specimen";

        private EntryTemplate arctosEntry;
        private EntryTemplate specimenNameEntry;
        private EntryTemplate specimenDescriptionEntry;

        public AddItemPage() { Initialize(); }

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

            var instructionLabel = new Label
            {
                Text = "Please enter specimen information:",
                Margin = new Thickness(24, 24, 24, 12),
                FontSize = 18
            };

            arctosEntry = new EntryTemplate(300, "Arctos ID (Enter only the numbers)");
            specimenNameEntry = new EntryTemplate(300, "Specimen Name");
            specimenDescriptionEntry = new EntryTemplate(300, "Specimen Description");

            var confirmButton = new TagAndTrackButton("Confirm Specimen", new Command(async () => await ConfirmSpecimen()), "check.png");

            Content = new ScrollView
            {
                Orientation = ScrollOrientation.Vertical,
                Content = new VerticalStackLayout
                {
                    Children =
                    {
                        header,
                        arctosEntry,
                        specimenNameEntry,
                        specimenDescriptionEntry,
                        confirmButton
                    }
                }
            };
        }

        private async Task ConfirmSpecimen()
        {
            if (specimenNameEntry.Text == "")
            {
                await Shell.Current.DisplayAlert("Error", "Specimen name must be entered.", "OK");
                return;
            }
            if (specimenDescriptionEntry.Text == "")
            {
                await Shell.Current.DisplayAlert("Error", "Specimen description must be entered.", "OK");
                return;
            }
            if (arctosEntry.Text == "")
            {
                await Shell.Current.DisplayAlert("Error", "Arctos ID must be entered.", "OK");
                return;
            }

            string arc = arctosEntry.Text;

            bool isNumber = int.TryParse(arc, out int id);

            if (!isNumber)
            {
                await Shell.Current.DisplayAlert("Error", "Arctos ID must be only numbers.", "Ok");
                return;
            }

            if (arc.Length != 6)
            {
                await Shell.Current.DisplayAlert("Error", "Arctos ID must be 6 digits.", "Ok");
                return;
            }

            var specimen = new SpecimenItem($"ARC-{id:D6}", specimenNameEntry.Text, specimenDescriptionEntry.Text);

            //TO DO: Look into where we want to check for duplicate ARC ID's (they may need to keep this functionality)
            await DbService.AddSpecimenAsync(specimen);

            await Shell.Current.DisplayAlert("Success!", "Specimen added to database!", "OK");

            var current = Shell.Current.CurrentPage;
            var pageType = current.GetType();

            await Shell.Current.Navigation.PushAsync((Page)Activator.CreateInstance(pageType));
            Shell.Current.Navigation.RemovePage(current);
        }
    }
}