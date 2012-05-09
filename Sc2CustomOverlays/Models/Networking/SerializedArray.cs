// -----------------------------------------------------------------------
// <copyright file="SerializedArray.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

namespace Sc2CustomOverlays.Models.Networking
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.IO;

    public static class SerializedArray
    {
        public static byte[] ToNetworkBytes(byte[] arr)
        {
            byte[] encodedString = arr;
            int strLen = encodedString.Length;

            byte[] networkBytes = new byte[4 + strLen];

            Array.Copy(BitConverter.GetBytes(strLen), networkBytes, 4);
            Array.Copy(encodedString, 0, networkBytes, 4, strLen);

            return networkBytes;
        }

        public static byte[] FromNetworkBytes(Stream s)
        {
            byte[] sizeBuffer = new byte[4];
            StreamHelper.ForceReadAll(s, sizeBuffer, 0, 4);
            
            int strLen = BitConverter.ToInt32(sizeBuffer, 0);
            if (strLen > 0)
            {
                byte[] arrBuffer = new byte[strLen];
                StreamHelper.ForceReadAll(s, arrBuffer, 0, strLen);
                return arrBuffer;
            }

            return new byte[0];
        }
    }
}
