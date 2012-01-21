using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sc2CustomOverlays.Code.Networking.Encryption;

namespace Sc2CustomOverlays.Code.Networking.Control.Commands
{
    public class OverlaySettingLoadedCommand : ControlCommand
    {
        #region Singleton Requirements
            protected static OverlaySettingLoadedCommand _instance = new OverlaySettingLoadedCommand();
            public static OverlaySettingLoadedCommand Instance { get { return _instance; } }

            protected OverlaySettingLoadedCommand()
            {

            }
        #endregion


            // Returns CommandResult (see below) that holds success/failure and implementation specific data.
            //  Return Data: 1
            public override CommandResult HandleCommand(EncryptedNetworkStream ns)
            {

                return new CommandResult(true);
            }

            // Returns whether the command sent successfully or not.
            //  In Parameters: 1
            //   Dictionary<string, 
            public override bool SendCommand(EncryptedNetworkStream ns, Dictionary<string, object> parameters = null)
            {
                return true;
            }
        
    }
}
