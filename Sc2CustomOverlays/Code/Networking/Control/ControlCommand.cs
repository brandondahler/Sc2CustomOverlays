using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using Sc2CustomOverlays.Code.Networking.Encryption;

namespace Sc2CustomOverlays.Code.Networking.Control.Commands
{
    public abstract class ControlCommand
    {
        // For each command type, add its name without Command at the end here.
        public enum Command
        {
            AvailableSettingsRequest = 0,
            AvailableSettingsResponse,
            OverlaySettingSelect,
            OverlaySettingLoaded,
            DownloadSettingRequest,
            DownloadSettingResponse,
            UpdateVariable,
            Close
        }

        // Require child commands be singleton-style
        protected ControlCommand()
        { 
        }

        /*
         * Parameters are implementation specific parameters, see the implementation for more info on a specific command
         * Both can throw CommandExceptions.
         */

        // Returns CommandResult (see below) that holds success/failure and implementation specific data.
        public abstract CommandResult HandleCommand(EncryptedNetworkStream ns);
        
        // Returns whether the command sent successfully or not.
        //  Note: parameters can be left null on any call, implementations should take this into consideration.
        public abstract bool SendCommand(EncryptedNetworkStream ns, Dictionary<string, object> parameters = null);
    }

    public class CommandResult
    {
        #region Success
            private bool _success;
            public bool Success { get { return _success; } }
        #endregion

        // Returned data, entries are command specific, see the implementation for more info on a specific command.
        #region Data
            private static Dictionary<string, object> _data = new Dictionary<string, object>();
            public Dictionary<string, object> Data { get { return _data; } }
        #endregion

        public CommandResult(bool rSuccess, Dictionary<string, object> rData = null)
        {
            _success = rSuccess;
            
            if (rData != null)
                _data = rData;
        }
    }
}
