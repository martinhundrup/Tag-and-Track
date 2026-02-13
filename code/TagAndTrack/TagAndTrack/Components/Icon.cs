using System;
using System.Collections.Generic;
using System.Text;

namespace TagAndTrack.Components
{
    public class Icon
    {
        public string SvgPath { get; set; }
        public Color Tint { get; set; }

        public Icon(string svgPath, Color tint)
        {
            SvgPath = svgPath;
            Tint = tint;
        }
    }
}
