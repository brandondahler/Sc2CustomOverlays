// -----------------------------------------------------------------------
// <copyright file="StreamHelper.cs" company="">
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

    public static class StreamHelper
    {

        public static void ForceReadAll(Stream s, byte[] buffer, int offset, int size)
        {
            while (size > 0)
            {
                int readSize = s.Read(buffer, offset, size);

                if (readSize <= 0)
                    throw new Exception("Unable to read from stream.");

                size -= readSize;
            }
        }

    }
}
