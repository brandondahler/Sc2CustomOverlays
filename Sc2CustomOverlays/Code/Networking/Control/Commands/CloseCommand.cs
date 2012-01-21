using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using Sc2CustomOverlays.Code.Networking.Encryption;

namespace Sc2CustomOverlays.Code.Networking.Control.Commands
{
    public class CloseCommand : ControlCommand
    {
        #region Singleton Requirements
            protected static CloseCommand _instance = new CloseCommand();
            public static CloseCommand Instance { get { return _instance; } }

            protected CloseCommand()
            {

            }
        #endregion

        // Returns CommandResult (see below) that holds success/failure and implementation specific data.
        //  Return Data: 0
        public override CommandResult HandleCommand(EncryptedNetworkStream ns)
        {   
            return new CommandResult(true);
        }

        // Returns whether the command sent successfully or not.
        //  In Parameters: 0
        public override bool SendCommand(EncryptedNetworkStream ns, Dictionary<string, object> parameters = null)
        {
            try
            {
                byte[] bufferBytes = new byte[] { (byte)ControlCommand.Command.Close };
                ns.Write(bufferBytes, 0, bufferBytes.Length);
            } catch (Exception) {
                return false;
            }

            return true;
        }
    }
}
