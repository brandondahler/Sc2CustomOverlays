using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Sc2CustomOverlays.ViewModel.OverlayVariables;

namespace Sc2CustomOverlays.Models
{

    public class VariableEvaluator
    {
        // Call this on a string of text with a given variable dictionary to recursively evaluate the final value of the string.
        public static string ReplaceVariables(string text, Dictionary<string, OverlayVariableBaseModel> variableDictionary)
        {
            Regex rVariables = new Regex("#(.*?)#");
            MatchCollection variables = rVariables.Matches(text);

            VariableEvaluator ve = new VariableEvaluator(variableDictionary);
            text = rVariables.Replace(text, new MatchEvaluator(ve.EvaluateMatch));

            return text;
        }

        //
        // This class can only be instantiatied through the static function above.
        //

        private Dictionary<string, OverlayVariableBaseModel> VariableDictionary = null;

        private VariableEvaluator(Dictionary<string, OverlayVariableBaseModel> variableDict)
        {
            VariableDictionary = variableDict;
        }


        // Evaluate each match found, recursively replacing variables.  To be used as a MatchEvaluator.
        private string EvaluateMatch(Match m)
        {
            string varName = m.Groups[1].Value;

            // Avoid variables referencing themselves
            if (varName != "")
            {
                if (VariableDictionary.ContainsKey(varName))
                {
                    Dictionary<string, OverlayVariableBaseModel> newDictionary = new Dictionary<string, OverlayVariableBaseModel>(VariableDictionary);
                    newDictionary.Remove(varName);

                    return ReplaceVariables(VariableDictionary[varName].StringValue, newDictionary);
                }

            } else {
                // Only in case of ##
                return "#";
            }

            return m.Value;
        }

    }
}
