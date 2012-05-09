using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.IO;

namespace Sc2CustomOverlays.Models.Networking
{
    public static class SerializedString
    {

        public static byte[] ToNetworkBytes(string str)
        {
            byte[] encodedString = System.Text.Encoding.UTF8.GetBytes(str);
            return SerializedArray.ToNetworkBytes(encodedString);
        }

        public static string FromNetworkBytes(Stream s)
        {
            byte[] stringArray = SerializedArray.FromNetworkBytes(s);
            return System.Text.Encoding.UTF8.GetString(stringArray, 0, stringArray.Length);
        }

    }
}
