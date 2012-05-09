// -----------------------------------------------------------------------
// <copyright file="AesStream.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

namespace Sc2CustomOverlays.Models.Networking.Encryption
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.IO;
    using System.Security.Cryptography;
    using System.Net.Sockets;

    public class AESStream : Stream
    {
        private RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider();

        private Stream baseStream;

        AesCryptoServiceProvider aesKey;

        private byte[] recvBufferExtra = null;

        public AESStream(Stream s, AesCryptoServiceProvider aKey)
        {
            baseStream = s;

            aesKey = aKey;
        }

        #region Read and Write
            public override int Read(byte[] buffer, int offset, int size)
            {
                int totalCopied = 0;

                // If there's extra data already read, use it first
                if (recvBufferExtra != null)
                {
                    // If we can fulfill without receiving any new data, do so
                    if (size <= recvBufferExtra.Length)
                    {
                        Array.Copy(recvBufferExtra, 0, buffer, offset, size);

                        if (recvBufferExtra.Length > size)
                        {
                            byte[] remainingExtra = new byte[recvBufferExtra.Length - size];
                            Array.Copy(recvBufferExtra, size, remainingExtra, 0, recvBufferExtra.Length - size);
                            recvBufferExtra = remainingExtra;
                        } else {
                            recvBufferExtra = null;
                        }

                        return size;
                    }

                    // Otherwise, fulfill out of the extra buffer and then continue on
                    Array.Copy(recvBufferExtra, 0, buffer, offset, size);
                    totalCopied += recvBufferExtra.Length;

                    offset += recvBufferExtra.Length;
                    size -= recvBufferExtra.Length;
                    recvBufferExtra = null;
                }


                while (size > 0)
                {
                    byte[] totalBlockSizeBuffer = new byte[4];
                    StreamHelper.ForceReadAll(baseStream, totalBlockSizeBuffer, 0, 4);
                    int totalBlockSize = BitConverter.ToInt32(totalBlockSizeBuffer, 0);

                    byte[] encryptedBuffer = new byte[totalBlockSize];
                    StreamHelper.ForceReadAll(baseStream, encryptedBuffer, 0, encryptedBuffer.Length);

                    ICryptoTransform decryptor = aesKey.CreateDecryptor();
                    byte[] decryptedBuffer = decryptor.TransformFinalBlock(encryptedBuffer, 0, encryptedBuffer.Length);

                    if (size >= decryptedBuffer.Length)
                    {
                        Array.Copy(decryptedBuffer, 0, buffer, offset, decryptedBuffer.Length);
                        totalCopied += decryptedBuffer.Length;

                        offset += decryptedBuffer.Length;
                        size -= decryptedBuffer.Length;
                    } else {
                        // Else copy the requested data and add only the extra data to a new extra buffer
                        Array.Copy(decryptedBuffer, 0, buffer, offset, size);
                        totalCopied += size;

                        recvBufferExtra = new byte[decryptedBuffer.Length - size];
                        Array.Copy(decryptedBuffer, size, recvBufferExtra, 0, decryptedBuffer.Length - size);

                        offset += size;
                        size = 0;
                    }

                }

                return totalCopied;
            }

            public override void Write(byte[] buffer, int offset, int size)
            {
                ICryptoTransform encryptor = aesKey.CreateEncryptor();
                byte[] encryptedBuffer = encryptor.TransformFinalBlock(buffer, offset, size);

                baseStream.Write(BitConverter.GetBytes(encryptedBuffer.Length), 0, 4);
                baseStream.Write(encryptedBuffer, 0, encryptedBuffer.Length);
            }
        #endregion

        #region Required Overrides
            public override long Position
            {
                get { return baseStream.Position; }
                set { baseStream.Position = value; }
            } 
   
            public override long Length
            {
                get { return baseStream.Length; }
            }

            public override bool CanRead
            {
                get { return baseStream.CanRead; }
            }

            public override bool CanSeek
            {
                get { return baseStream.CanSeek; }
            }

            public override bool CanWrite
            {
                get { return baseStream.CanWrite; }
            }

            public override bool CanTimeout
            {
                get { return baseStream.CanTimeout; }
            }

            public override int ReadTimeout
            {
                get { return baseStream.ReadTimeout; }
                set { baseStream.ReadTimeout = value; }
            }

            public override int WriteTimeout
            {
                get { return baseStream.WriteTimeout; }
                set { baseStream.WriteTimeout = value; }
            }

            

            public override void Close() { baseStream.Close(); }
            public override void SetLength(long value) { baseStream.SetLength(value); }
            public override long Seek(long offset, SeekOrigin origin) { return baseStream.Seek(offset, origin); }
            public override void Flush() { baseStream.Flush(); }


        #endregion

    }
}
