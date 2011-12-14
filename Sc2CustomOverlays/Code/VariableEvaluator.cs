using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

using Sc2CustomOverlays.Code.OverlayVariables;

namespace Sc2CustomOverlays.Code
{

    class VariableEvaluator
    {
        public Dictionary<string, OverlayVariable> VariableDictionary = null;

        public VariableEvaluator(Dictionary<string, OverlayVariable> variableDict)
        {
            VariableDictionary = variableDict;
        }


        public string EvaluateMatch(Match m)
        {
            string varName = m.Groups[1].Value;

            // Avoid variables referencing themselves
            if (varName != "")
            {
                if (VariableDictionary.ContainsKey(varName))
                {
                    Dictionary<string, OverlayVariable> newDictionary = new Dictionary<string, OverlayVariable>(VariableDictionary);
                    newDictionary.Remove(varName);

                    return ReplaceVariables((string) VariableDictionary[varName].Value, newDictionary);
                }

            }
            else
            {
                // Only in case of ##
                return "#";
            }

            return m.Value;
        }

        public static string ReplaceVariables(string text, Dictionary<string, OverlayVariable> variableDictionary)
        {
            Regex rVariables = new Regex("#(.*?)#");
            MatchCollection variables = rVariables.Matches(text);

            VariableEvaluator ve = new VariableEvaluator(variableDictionary);
            text = rVariables.Replace(text, new MatchEvaluator(ve.EvaluateMatch));

            return text;
        }

    }
}
