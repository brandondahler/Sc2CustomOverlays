using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sc2CustomOverlays.Code.Networking.Encryption;
using System.IO;

namespace Sc2CustomOverlays.Code.Networking.Control.Commands
{
    public class AvailableSettingsResponseCommand : ControlCommand
    {
        #region Singleton Requirements
            protected static AvailableSettingsResponseCommand _instance = new AvailableSettingsResponseCommand();
            public static AvailableSettingsResponseCommand Instance { get { return _instance; } }

            protected AvailableSettingsResponseCommand()
            {

            }
        #endregion

        // Returns CommandResult (see below) that holds success/failure and implementation specific data.
        //  Return Data: 1
        //   List<AvailableOverlaySetting> remoteSettings
        public override CommandResult HandleCommand(EncryptedNetworkStream ns)
        {
            Dictionary<string, object> resultData = new Dictionary<string,object>();
            List<AvailableOverlaySetting> remoteSettings = new List<AvailableOverlaySetting>();

            try
            {
                // Read length of the list
                byte[] settingsListSizeBytes = new byte[4];
                ns.ForceReadAll(settingsListSizeBytes, 0, 4);
                int settingsListSize = BitConverter.ToInt32(settingsListSizeBytes, 0);

                // For each element, read name and path and create an AvailableOverlaySetting for each
                for (int x = 0; x < settingsListSize; ++x)
                {
                    AvailableOverlaySetting aos = new AvailableOverlaySetting();

                    aos.Name = SerializedString.FromNetworkBytes(ns);
                    aos.Path = SerializedString.FromNetworkBytes(ns);
                    aos.Local = false;
                    //aos.IsCurrent = (OverlaySettings.Instance.Location.FullName == Path.Combine(OverlaySettings.OverlaysTempBasePath.FullName, aos.Path));

                    remoteSettings.Add(aos);
                }
            } catch (Exception) {
                return new CommandResult(false);
            }


            resultData["remoteSettings"] = remoteSettings;
            return new CommandResult(true, resultData);
        }

        // Returns whether the command sent successfully or not.
        //  In Parameters: 1
        //   List<AvailableOverlaySetting> availableSettings
        public override bool SendCommand(EncryptedNetworkStream ns, Dictionary<string, object> parameters = null)
        {
            // Validate that parameters is valid.
            if (parameters == null)
                throw new CommandException("AvailableSettingsResponseCommand", true, CommandException.Reason.MissingParameter);

            List<AvailableOverlaySetting> availableSettings = null;

            // Load parameters into local variable
            try
            {
                 availableSettings = (List<AvailableOverlaySetting>) parameters["availableSettings"];
            } catch (KeyNotFoundException) {
                throw new CommandException("AvailableSettingsResponseCommand", true, CommandException.Reason.MissingParameter);
            } catch (InvalidCastException) {
                throw new CommandException("AvailableSettingsResponseCommand", true, CommandException.Reason.InvalidParameterType);
            }
            
            // List size
            byte[] settingsListSize = new byte[4];
            settingsListSize = BitConverter.GetBytes(availableSettings.Count);

            // Bytes for each of the availableSettings
            byte[][]  settingsList = new byte[availableSettings.Count][];
            int totalListBytesSize = 0;
            
            // Serialize each AvailableOverlaySetting to its bytes
            for (int x = 0; x < availableSettings.Count; ++x)
            {
                AvailableOverlaySetting aos = availableSettings[x];
                byte[] serializedName = SerializedString.ToNetworkBytes(aos.Name);
                byte[] serializedPath = SerializedString.ToNetworkBytes(aos.Path);

                settingsList[x] = new byte[serializedName.Length + serializedPath.Length];
                Array.Copy(serializedName, 0, settingsList[x], 0, serializedName.Length);
                Array.Copy(serializedPath, 0, settingsList[x], serializedName.Length, serializedPath.Length);

                // Keep track of the total length of the bytes put together are.
                totalListBytesSize += settingsList[x].Length;
            }

            // Create final buffer
            byte[] bufferBytes = new byte[1 + 4 + totalListBytesSize];

            bufferBytes[0] = (byte)ControlCommand.Command.AvailableSettingsResponse;
            Array.Copy(settingsListSize, 0, bufferBytes, 1, 4);

            // Concatenate all the AvailableOverlaySetting bytes together
            int curOffset = 5;
            foreach (byte[] setting in settingsList)
            {
                Array.Copy(setting, 0, bufferBytes, curOffset, setting.Length);
                curOffset += setting.Length;
            }

            // Write out the bytes to the network socket
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
