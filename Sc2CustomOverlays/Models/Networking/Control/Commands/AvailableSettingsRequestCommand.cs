using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Sc2CustomOverlays.Models.Networking.Control.Commands
{
    public class AvailableSettingsRequestCommand : ControlCommand
    {
        #region Singleton Requirements
            protected static AvailableSettingsRequestCommand _instance = new AvailableSettingsRequestCommand();
            public static AvailableSettingsRequestCommand Instance { get { return _instance; } }

            protected AvailableSettingsRequestCommand()
            {

            }
        #endregion

        // Returns CommandResult (see below) that holds success/failure and implementation specific data.
        //  Return Data: 0
        public override CommandResult HandleCommand(Stream ns)
        {
            return new CommandResult(true);
        }

        // Returns whether the command sent successfully or not.
        //  In Parameters: 0
        public override bool SendCommand(Stream ns, Dictionary<string, object> parameters = null)
        {
            try
            {
                byte[] bufferBytes = new byte[] { (byte)ControlCommand.Command.AvailableSettingsRequest };
                ns.Write(bufferBytes, 0, bufferBytes.Length);
            } catch (Exception) {
                return false;
            }

            return true;
        }
    }
}
