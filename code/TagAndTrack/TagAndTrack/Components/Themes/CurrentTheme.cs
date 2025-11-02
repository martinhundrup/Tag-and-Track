using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TagAndTrack.Components
{
    /// <summary>
    /// Class that holds the current theme of the application.
    /// </summary>
    public class CurrentTheme : INotifyPropertyChanged
    {
        private static readonly CurrentTheme _instance = new CurrentTheme();
        public static CurrentTheme Instance => _instance;

        private Theme _theme = new LightTheme();

        /// <summary>
        /// Creates a new instance of the <see cref="CurrentTheme"/> class.
        /// </summary>
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

        /// <summary>
        /// Switches the current theme between light and dark.
        /// </summary>
        public void SwitchTheme()
        {
            Theme = Theme is LightTheme ? new DarkTheme() : new LightTheme();
        }

    }
}
