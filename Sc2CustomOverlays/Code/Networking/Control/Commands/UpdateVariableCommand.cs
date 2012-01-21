using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using Sc2CustomOverlays.Code.Networking.Encryption;

namespace Sc2CustomOverlays.Code.Networking.Control.Commands
{
    class UpdateVariableCommand: ControlCommand
    {
        #region Singleton Requirements
            protected static UpdateVariableCommand _instance = new UpdateVariableCommand();
            public static UpdateVariableCommand Instance { get { return _instance; } }

            protected UpdateVariableCommand()
            {

            }
        #endregion

        private uint lastStreamId = 0xFFFFFFFF;
        private string lastVariableName = null;
        private string lastVariableValue = null;

        // Returns CommandResult (see below) that holds success/failure and implementation specific data.
        //  Return Data: 2
        //      string variableName - Name of updated variable
        //      string variableValue - Value of updated variable
        public override CommandResult HandleCommand(EncryptedNetworkStream ns)
        {
            Dictionary<string, object> resultData = new Dictionary<string, object>();

            string variableName = SerializedString.FromNetworkBytes(ns);
            string variableValue = SerializedString.FromNetworkBytes(ns);

            resultData["variableName"] = variableName;
            resultData["variableValue"] = variableValue;

            lastStreamId = ns.Id;
            lastVariableName = variableName;
            lastVariableValue = variableValue;
            
            return new CommandResult(true, resultData);
        }

        // Returns whether the command sent successfully or not.
        //  In Parameters: 2
        //      string variableName - Name of updated variable
        //      string variableValue - Value of updated variable
        public override bool SendCommand(EncryptedNetworkStream ns, Dictionary<string, object> parameters = null)
        {
            if (parameters == null)
                throw new CommandException("UpdateVariableCommand", true, CommandException.Reason.MissingParameter);

            string variableName = null;
            string variableValue = null;

            try
            {
                variableName = (string) parameters["variableName"];
                variableValue = (string) parameters["variableValue"];
            } catch (KeyNotFoundException) {
                throw new CommandException("UpdateVariableCommand", true, CommandException.Reason.MissingParameter);
            } catch (InvalidCastException) {
                throw new CommandException("UpdateVariableCommand", true, CommandException.Reason.InvalidParameterType);
            }

            // Don't resend what they just told us if it was on the same stream
            if (ns.Id == lastStreamId && lastVariableName != null && lastVariableValue != null &&
                lastVariableName == variableName && lastVariableValue == variableValue)
            {
                return false;
            }

            byte[] nameBytes = SerializedString.ToNetworkBytes(variableName);
            byte[] valueBytes = SerializedString.ToNetworkBytes(variableValue);

            byte[] bufferBytes = new byte[1 + nameBytes.Length + valueBytes.Length];

            bufferBytes[0] = (byte)ControlCommand.Command.UpdateVariable;
            Array.Copy(nameBytes, 0, bufferBytes, 1, nameBytes.Length);
            Array.Copy(valueBytes, 0, bufferBytes, 1 + nameBytes.Length, valueBytes.Length);

            try
            {
                ns.Write(bufferBytes, 0, bufferBytes.Length);
            } catch (Exception) {
                return false;
            }

            return true;
        }
    }
}
