using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sc2CustomOverlays.Code.Networking.Encryption;

namespace Sc2CustomOverlays.Code.Networking.Control.Commands
{
    public class OverlaySettingSelectCommand : ControlCommand
    {
        #region Singleton Requirements
            protected static OverlaySettingSelectCommand _instance = new OverlaySettingSelectCommand();
            public static OverlaySettingSelectCommand Instance { get { return _instance; } }

            protected OverlaySettingSelectCommand()
            {

            }
        #endregion

        // Returns CommandResult (see below) that holds success/failure and implementation specific data.
        //  Return Data: 2
        //   bool remote
        //   string selectedPath
        public override CommandResult HandleCommand(EncryptedNetworkStream ns)
        {
            Dictionary<string, object> commandData = new Dictionary<string, object>();

            // Read serverRemoteByte
            byte[] serverRemoteByte = new byte[1];
            ns.ForceReadAll(serverRemoteByte, 0, 1);
            commandData["remote"] = BitConverter.ToBoolean(serverRemoteByte, 0);

            // Read selectedPath
            commandData["selectedPath"] = SerializedString.FromNetworkBytes(ns);

            return new CommandResult(true, commandData);
        }

        // Returns whether the command sent successfully or not.
        //  In Parameters: 1
        //   AvailableOverlaySetting selectedSetting 
        public override bool SendCommand(EncryptedNetworkStream ns, Dictionary<string, object> parameters = null)
        {
            // Validate that parameters is valid.
            if (parameters == null)
                throw new CommandException("OverlaySettingSelect", true, CommandException.Reason.MissingParameter);

            AvailableOverlaySetting selectedSetting = null;

            // Load parameters into local variable
            try
            {
                selectedSetting = (AvailableOverlaySetting) parameters["selectedSetting"];
            } catch (KeyNotFoundException) {
                throw new CommandException("OverlaySettingSelect", true, CommandException.Reason.MissingParameter);
            } catch (InvalidCastException) {
                throw new CommandException("OverlaySettingSelect", true, CommandException.Reason.InvalidParameterType);
            }

            
            byte[] serverRemoteByte = BitConverter.GetBytes(selectedSetting.Local);
            byte[] settingPath = SerializedString.ToNetworkBytes(selectedSetting.Path);

            byte[] bufferBytes = new byte[1 + 1 + settingPath.Length];
            bufferBytes[0] = (byte) ControlCommand.Command.OverlaySettingSelect;
            bufferBytes[1] = serverRemoteByte[0];
            Array.Copy(settingPath, 0, bufferBytes, 2, settingPath.Length);

            ns.Write(bufferBytes, 0, bufferBytes.Length);

            return true;
        }
    }
}
