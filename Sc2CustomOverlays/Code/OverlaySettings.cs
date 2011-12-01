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
        private string startDirectory = "/";
        private List<Overlay> myOverlays = new List<Overlay>();
        private Dictionary<string, OverlayVariable> variableDictionary = new Dictionary<string, OverlayVariable>();
        private Dictionary<string, string> variableGroups = new Dictionary<string, string>();
        

        public OverlaySettings(string file)
        {
            fileLocation = file;
            startDirectory = file.Substring(0, file.LastIndexOf("\\") + 1);
            FromXML(file);
        }

        public List<Overlay> GetOverlays()
        {
            return myOverlays;
        }

        public IEnumerable<OverlayVariable> GetVariables()
        {
            return variableDictionary.Values;
        }

        public Dictionary<string, string> GetVariableGroups()
        {
            return variableGroups;
        }

        public void FromXML(string file)
        {
            XmlDocument xDoc = new XmlDocument();
            xDoc.Load(file);

            XmlNode osNode = xDoc.SelectSingleNode("/OverlaySettings");

            XmlNodeList glNode = osNode.SelectNodes("Groups/Group");
            if (glNode != null)
            {
                foreach (XmlNode gNode in glNode)
                    variableGroups.Add(gNode.Attributes.GetNamedItem("name").Value, gNode.Attributes.GetNamedItem("label").Value);
            }

            variableDictionary.Clear();
            XmlNode vlNode = osNode.SelectSingleNode("Variables");
            if (vlNode != null)
                variableDictionary = OverlayVariable.ProcessVariables(vlNode, variableGroups.Keys);

            foreach (OverlayVariable ov in variableDictionary.Values)
            {
                ov.Updated += new UpdatedEventHandler(VariableUpdated);
            }

            XmlNodeList oNodes = osNode.SelectNodes("Overlays/Overlay");
            if (oNodes != null)
            {
                foreach (XmlNode oNode in oNodes)
                {
                    Overlay o = new Overlay(startDirectory);
                    o.FromXML(oNode);
                    o.SetVariableDictionary(variableDictionary);
                    myOverlays.Add(o);
                }
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
