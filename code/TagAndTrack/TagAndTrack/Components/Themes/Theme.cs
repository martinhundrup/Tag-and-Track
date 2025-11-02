using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TagAndTrack.Components
{
    /// <summary>
    /// Theme abstract class to define themes for the application.
    /// </summary>
    public abstract class Theme
    {
        /// <summary>
        /// The color for the background.
        /// </summary>
        public Color? Background { get; init; }

        /// <summary>
        /// The color for the text.
        /// </summary>
        public Color? Text { get; init; }

        /// <summary>
        /// The color for the borders.
        /// </summary>
        public Color? Borders { get; init; }
    }
}
