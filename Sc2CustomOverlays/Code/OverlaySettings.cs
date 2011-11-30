using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace Sc2CustomOverlays
{
    public class OverlaySettings
    {
        private string fileLocation = null;
        private List<Overlay> myOverlays = new List<Overlay>();
        private Dictionary<string, OverlayVariable> variableDictionary = new Dictionary<string, OverlayVariable>();
        

        public OverlaySettings(string file)
        {
            fileLocation = file;
            FromXML(file);
        }

        public List<Overlay> GetOverlays()
        {
            return myOverlays;
        }

        public Dictionary<string, OverlayVariable>.ValueCollection GetVariables()
        {
            return variableDictionary.Values;
        }

        public void FromXML(string file)
        {
            XmlDocument xDoc = new XmlDocument();
            xDoc.Load(file);

            XmlNode osNode = xDoc.SelectSingleNode("/OverlaySettings");

            variableDictionary.Clear();
            XmlNode vlNode = osNode.SelectSingleNode("Variables");
            foreach (XmlNode vNode in vlNode.ChildNodes)
            {
                OverlayVariable ov = null;
                switch (vNode.LocalName)
                {
                    case "Counter":
                        ov = new OverlayCounter();
                        break;

                    case "DropDown":
                        ov = new OverlayDropDown();
                        break;

                    case "String":
                    default:
                        ov = new OverlayString();
                        break;
                }
                ov.FromXML(vNode);

                if (ov.Name != null)
                {
                    variableDictionary.Add(ov.Name, ov);
                    ov.Updated += new UpdatedEventHandler(VariableUpdated);
                }
            }


            XmlNodeList oNodes = osNode.SelectNodes("Overlays/Overlay");
            foreach (XmlNode oNode in oNodes)
            {
                Overlay o = new Overlay();
                o.FromXML(oNode);
                o.SetVariableDictionary(variableDictionary);
                myOverlays.Add(o);
            }

        }

        private void VariableUpdated()
        {
            foreach (Overlay o in myOverlays)
            {
                o.UpdateVariables();
            }
        }

        
    }
}
