// QrCodeView.cs
using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Media;
using ZXing.Net.Maui;
using ZXing.Net.Maui.Controls;

namespace TagAndTrack.Components
{
    public class QrCodeView : ContentView
    {
        public static readonly BindableProperty ValueProperty =
            BindableProperty.Create(nameof(Value), typeof(string), typeof(QrCodeView), null, propertyChanged: OnChanged);

        public static readonly BindableProperty SizeProperty =
            BindableProperty.Create(nameof(Size), typeof(double), typeof(QrCodeView), 256d, propertyChanged: OnChanged);

        public static readonly BindableProperty ForegroundProperty =
            BindableProperty.Create(nameof(Foreground), typeof(Color), typeof(QrCodeView), Colors.Black, propertyChanged: OnChanged);

        public string? Value { get => (string?)GetValue(ValueProperty); set => SetValue(ValueProperty, value); }
        public double Size { get => (double)GetValue(SizeProperty); set => SetValue(SizeProperty, value); }
        public Color Foreground { get => (Color)GetValue(ForegroundProperty); set => SetValue(ForegroundProperty, value); }

        private readonly BarcodeGeneratorView _generator;

        public QrCodeView()
        {
            _generator = new BarcodeGeneratorView
            {
                Format = BarcodeFormat.QrCode,
                WidthRequest = 256,
                HeightRequest = 256,
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center
            };

            // Tap = open native share sheet
            var tap = new TapGestureRecognizer();
            tap.Tapped += async (_, __) => await ShareAsync();
            GestureRecognizers.Add(tap);

            Content = new Grid { Children = { _generator } };
        }

        private static void OnChanged(BindableObject bindable, object oldValue, object newValue)
        {
            ((QrCodeView)bindable).ApplyProps();
        }

        private void ApplyProps()
        {
            _generator.Value = Value ?? string.Empty;
            
            var s = Size; // your bindable double

            // pin both the content view and the generator to s x s
            WidthRequest = s;
            HeightRequest = s;

            _generator.WidthRequest = s;
            _generator.HeightRequest = s;

            _generator.ForegroundColor = Foreground;
            // For a white quiet zone, set Padding on this QrCodeView (e.g., Padding = 4)
        }

        // Capture this view as PNG and invoke the system share sheet
        public async Task ShareAsync(string title = "QR Code")
        {
            var shot = await this.CaptureAsync();
            if (shot == null) return;

            using var src = await shot.OpenReadAsync();
            var path = Path.Combine(FileSystem.CacheDirectory, $"qr_{Guid.NewGuid():N}.png");
            using (var fs = File.Create(path))
                await src.CopyToAsync(fs);

            await Share.Default.RequestAsync(new ShareFileRequest(title, new ShareFile(path)));
        }
    }
}
