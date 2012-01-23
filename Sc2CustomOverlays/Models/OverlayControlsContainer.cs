using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace Sc2CustomOverlays.Models
{
    public class OverlayControlsContainer
    {
        public UIElement Label { get; set; }
        public UIElement Modifier { get; set; }
        public UIElement Save { get; set; }
        public UIElement Reset { get; set; }
        public OverlayVariableGroup Group { get; set; }
    }
}
