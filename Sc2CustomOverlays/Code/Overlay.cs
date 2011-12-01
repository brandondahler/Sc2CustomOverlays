using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Xml;

namespace Sc2CustomOverlays
{
    public class Overlay
    {

        protected string startDirectory = "/";

        private List<OverlayItem> overlayItems = new List<OverlayItem>();
        private Dictionary<string, OverlayVariable> variableDictionary = null;

        public Overlay(string startDir)
        {
            startDirectory = startDir;
        }

        public List<FrameworkElement> GetOverlayControls()
        {
            List<FrameworkElement> controlList = new List<FrameworkElement>();

            foreach (OverlayItem oi in overlayItems)
            {
                controlList.Add(oi.GetElement());
            }

            return controlList;
        }

        public void FromXML(XmlNode xOverlayNode)
        {
            foreach (XmlNode xNode in xOverlayNode.ChildNodes)
            {
                OverlayItem oi = null;

                switch (xNode.LocalName)
                {
                    case "Image":
                        oi = new OverlayImage(startDirectory);
                        break;
                    case "Text":
                        oi = new OverlayText();
                        break;
                    case "Gradient":
                        oi = new OverlayGradient();
                        break;
                }

                if (oi != null)
                {
                    oi.FromXML(xNode);
                    overlayItems.Add(oi);
                }
            }
        }

        public void SetVariableDictionary(Dictionary<string, OverlayVariable> vDict)
        {
            variableDictionary = vDict;
            UpdateVariables();
        }


        public void UpdateVariables()
        {
            foreach (OverlayItem oi in overlayItems)
            {
                oi.UpdateVariables(variableDictionary);
            }
        }

    }
}
