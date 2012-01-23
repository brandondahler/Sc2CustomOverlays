using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sc2CustomOverlays.Models
{
    public class AvailableOverlaySetting
    {
        public string Name { get; set; }
        public bool Local { get; set; }
        public string Path { get; set; }
        public bool IsCurrent { get; set; }
    }
}
