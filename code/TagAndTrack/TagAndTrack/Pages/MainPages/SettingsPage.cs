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

            var qr = new QrCodeView[]
                { new QrCodeView {
                    Value = "Specimen:1",
                    Size = 220,
                    Padding = 4,          // quiet zone in pixels
                    Foreground = Colors.Black
                },
                new QrCodeView {
                    Value = "Specimen:2",
                    Size = 220,
                    Padding = 4,          // quiet zone in pixels
                    Foreground = Colors.Black
                },
                new QrCodeView {
                    Value = "Loan:1",
                    Size = 220,
                    Padding = 4,          // quiet zone in pixels
                    Foreground = Colors.Black
                },
                new QrCodeView {
                    Value = "Loan:2",
                    Size = 220,
                    Padding = 4,          // quiet zone in pixels
                    Foreground = Colors.Black
                },
            };

            Content = new VerticalStackLayout()
            {
                Children =
                {
                    header,
                    new HorizontalStackLayout()
                    {
                        qr[0],
                        qr[1],
                        qr[2],
                        qr[3],
                    }
                }
            };
        }
    }
}