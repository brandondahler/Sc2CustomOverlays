using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using Sc2CustomOverlays.Models.Networking.Encryption;

namespace Sc2CustomOverlays.Models.Networking
{
    public static class SerializedString
    {

        public static byte[] ToNetworkBytes(string str)
        {
            byte[] encodedString = System.Text.Encoding.UTF8.GetBytes(str);
            int strLen = encodedString.Length;

            byte[] networkBytes = new byte[4 + strLen];

            Array.Copy(BitConverter.GetBytes(strLen), networkBytes, 4);
            Array.Copy(encodedString, 0, networkBytes, 4, strLen);

            return networkBytes;
        }

        public static string FromNetworkBytes(EncryptedNetworkStream ns)
        {
            byte[] sizeBuffer = new byte[4];
            ns.ForceReadAll(sizeBuffer, 0, 4);
            
            int strLen = BitConverter.ToInt32(sizeBuffer, 0);
            if (strLen > 0)
            {
                byte[] stringBuffer = new byte[strLen];
                ns.ForceReadAll(stringBuffer, 0, strLen);

                return System.Text.Encoding.UTF8.GetString(stringBuffer, 0, strLen);
            }

            return "";
        }

    }
}
