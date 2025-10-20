using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TagAndTrack.Components
{
    public class CurrentTheme : INotifyPropertyChanged
    {
        private static readonly CurrentTheme _instance = new CurrentTheme();
        public static CurrentTheme Instance => _instance;

        private Theme _theme = new LightTheme();

        public Theme Theme
        {
            get => _theme;
            private set
            {
                if (_theme != value)
                {
                    _theme = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Theme)));
                }
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        public void SwitchTheme()
        {
            Theme = Theme is LightTheme ? new DarkTheme() : new LightTheme();
        }

    }
}
