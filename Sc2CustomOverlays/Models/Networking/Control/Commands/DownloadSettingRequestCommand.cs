using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sc2CustomOverlays.Models.Networking.Encryption;

namespace Sc2CustomOverlays.Models.Networking.Control.Commands
{
    public class DownloadSettingRequestCommand : ControlCommand
    {
        #region Singleton Requirements
            protected static DownloadSettingRequestCommand _instance = new DownloadSettingRequestCommand();
            public static DownloadSettingRequestCommand Instance { get { return _instance; } }

            protected DownloadSettingRequestCommand()
            {

            }
        #endregion

        // Returns CommandResult (see below) that holds success/failure and implementation specific data.
        //  Return Data: 1
        //   string settingPath
        public override CommandResult HandleCommand(EncryptedNetworkStream ns)
        {
            Dictionary<string, object> returnData = new Dictionary<string,object>();

            returnData["settingPath"] = SerializedString.FromNetworkBytes(ns);

            return new CommandResult(true, returnData);
        }

        // Returns whether the command sent successfully or not.
        //  In Parameters: 1
        //   string settingPath
        public override bool SendCommand(EncryptedNetworkStream ns, Dictionary<string, object> parameters = null)
        {
            // Validate that parameters is valid.
            if (parameters == null)
                throw new CommandException("DownloadSettingRequestCommand", true, CommandException.Reason.MissingParameter);

            string settingPath = null;

            // Load parameters into local variable
            try
            {
                settingPath = (string) parameters["settingPath"];
            } catch (KeyNotFoundException) {
                throw new CommandException("DownloadSettingRequestCommand", true, CommandException.Reason.MissingParameter);
            } catch (InvalidCastException) {
                throw new CommandException("DownloadSettingRequestCommand", true, CommandException.Reason.InvalidParameterType);
            }

            byte[] settingPathBytes = SerializedString.ToNetworkBytes(settingPath);
            byte[] bufferBytes = new byte[1 + settingPathBytes.Length];

            bufferBytes[0] = (byte) ControlCommand.Command.DownloadSettingRequest;
            Array.Copy(settingPathBytes, 0, bufferBytes, 1, settingPathBytes.Length);

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
