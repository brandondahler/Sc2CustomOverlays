using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;

namespace Sc2CustomOverlays.Models.Networking.Discovery
{
    namespace DiscoveryShared
    {
        
        // All shared constants should go here
        static class DiscoveryConstants
        {
            public const ushort ClientPort = 9998;
            public const ushort ServerPort = 9999;

            // Discovery requests should send DiscoveryRequest.Request alone.
            public enum Requests
            {
                Request = 0
            }

            // Discovery responses should respond with DiscoveryResponse.Response followed by a ServerInfo class.
            public enum Responses
            {
                Response = 0
            }
        }

        // Class used to transmit information about a server that is discoverable.
        public class DiscoveryServerInfo
        {
            public string Name
            {
                get { return _name; }
                set
                {
                    if (value.Length > 255)
                        _name = value.Substring(0, 255);
                    else
                        _name = value;
                }
            }
            public ushort Port;

            private string _name;

            // Convert data into sendable bytes
            public byte[] ToBytes()
            {
                byte[] byteArray = new byte[1 + Name.Length + 2];

                byteArray[0] = (byte)Name.Length;
                Array.Copy(Encoding.UTF8.GetBytes(Name.ToCharArray()), 0, byteArray, 1, Name.Length);
                Array.Copy(BitConverter.GetBytes(Port), 0, byteArray, 1 + Name.Length, 2);

                return byteArray;
            }

            // Parse given bytes,
            //  Returns ending index of the array or -1 for error
            public int FromBytes(byte[] byteArray, int index)
            {
                if (byteArray.Length < 1 + index)
                    return -1;

                int nameLength = (int) byteArray[index + 0];

                if (byteArray.Length < (index + 1 + nameLength + 2))
                    return -1;

                Name = Encoding.UTF8.GetString(byteArray, index + 1, nameLength);
                Port = BitConverter.ToUInt16(byteArray, index + 1 + nameLength);

                return index + 1 + nameLength + 2;
            }
        }

    }
}
