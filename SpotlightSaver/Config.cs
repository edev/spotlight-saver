using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpotlightSaver
{
    public class Config
    {
        public string LandscapeFolder { get; set; } = "Landscape";
        public string PortraitFolder { get; set; } = "Portrait";
        public string SquareFolder { get; set; } = "Square";

        public bool IncludeLandscape { get; set; } = true;
        public bool IncludePortrait { get; set; } = true;
        public bool IncludeSquare { get; set; } = true;

        public uint MinHeight { get; set; } = 1080;
        public uint MinWidth { get; set; } = 1080;
    }
}
