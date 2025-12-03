using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TagAndTrack.Components
{
    /// <summary>
    /// The light theme.
    /// </summary>
    public class LightTheme : Theme
    {
        /// <summary>
        /// Creates the light theme for the application.
        /// </summary>
        public LightTheme()
        {
            this.Background = Colors.White;
            this.Text = Colors.Black;
            this.Borders = Colors.Black;
        }
    }
}
