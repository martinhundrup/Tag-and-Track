using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TagAndTrack.Components
{
    /// <summary>
    /// The dark theme.
    /// </summary>
    public class DarkTheme : Theme
    {
        /// <summary>
        /// Creates the dark theme for the application.
        /// </summary>
        public DarkTheme()
        {
            Background = Colors.Black;
            Text = Colors.AntiqueWhite;
            Borders = Colors.White;
        }
    }
}
