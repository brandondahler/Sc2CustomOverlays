using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.IO;
using System.Security;
using System.Security.Cryptography;
using System.Threading;
using System.Security.Cryptography.X509Certificates;

namespace Sc2CustomOverlays.Models.Networking.Encryption
{
    public sealed class EncryptedNetworkStream : Stream, IDisposable
    {
        private NetworkStream baseStream;

        private RSAStream rsaStream = null;
        private AESStream aesStream = null;
        

        #region Construction and Authentication
            public EncryptedNetworkStream(NetworkStream ns, string password, bool client)
            {
                baseStream = ns;
                NegotiateAuthentication(password, client);
            }

            private void NegotiateAuthentication(string streamPassword, bool client)
            {
                int oldReadTimeout = baseStream.ReadTimeout;

                baseStream.ReadTimeout = 5000;

                
                RSACryptoServiceProvider localKeys;
                RSACryptoServiceProvider remoteKey;
        
                // Generate and send public key 
                {
                    localKeys = new RSACryptoServiceProvider();
                    byte[] publicBlob = localKeys.ExportCspBlob(false);
                    baseStream.Write(BitConverter.GetBytes(publicBlob.Length), 0, 4);
                    baseStream.Write(publicBlob, 0, publicBlob.Length);
                }

                // Receive remote public key                
                try 
                {
                    byte[] blobLen = new byte[4];
                    StreamHelper.ForceReadAll(baseStream, blobLen, 0, 4);

                    byte[] remoteBlob = new byte[BitConverter.ToInt32(blobLen, 0)];
                    StreamHelper.ForceReadAll(baseStream, remoteBlob, 0, remoteBlob.Length);

                    remoteKey = new RSACryptoServiceProvider();
                    remoteKey.ImportCspBlob(remoteBlob);
                } catch (Exception) {
                    throw new EncryptedNetworkStreamException(EncryptedNetworkStreamException.Reason.NetworkError);
                }

                rsaStream = new RSAStream(baseStream, localKeys, remoteKey);

                AesCryptoServiceProvider aesKey = new AesCryptoServiceProvider();
                aesKey.KeySize = 256;

                // Authenticate client/server
                byte[] passwordHash = MD5.Create().ComputeHash(ASCIIEncoding.UTF8.GetBytes(streamPassword));

                if (client)
                {
                    // If we're the client, send the password hash (encrypted)
                    rsaStream.Write(passwordHash, 0, 16);

                    // Read AES key
                    byte[] lenBuffer = new byte[4];
                    StreamHelper.ForceReadAll(rsaStream, lenBuffer, 0, 4);

                    byte[] aesKeyBuffer = new Byte[BitConverter.ToInt32(lenBuffer, 0)];
                    StreamHelper.ForceReadAll(rsaStream, aesKeyBuffer, 0, aesKeyBuffer.Length);

                    StreamHelper.ForceReadAll(rsaStream, lenBuffer, 0, 4);

                    byte[] aesIVBuffer = new Byte[BitConverter.ToInt32(lenBuffer, 0)];
                    StreamHelper.ForceReadAll(rsaStream, aesIVBuffer, 0, aesIVBuffer.Length);

                    aesKey.Key = aesKeyBuffer;
                    aesKey.IV = aesIVBuffer;

                } else {
                    // If we're the server, wait for them to send the password (encrypted)
                    byte[] receivedPasswordHash = new byte[16];
                    StreamHelper.ForceReadAll(rsaStream, receivedPasswordHash, 0, 16);


                    if (! passwordHash.SequenceEqual(receivedPasswordHash))
                        throw new EncryptedNetworkStreamException(EncryptedNetworkStreamException.Reason.InvalidAuthentication);
    
                    // Generate/send AES key
                    aesKey.GenerateKey();
                    aesKey.GenerateIV();

                    // Group length and key togeather to lower overhead

                    byte[] keyDataBuffer = SerializedArray.ToNetworkBytes(aesKey.Key);
                    rsaStream.Write(keyDataBuffer, 0, keyDataBuffer.Length);

                    byte[] keyIVBuffer = SerializedArray.ToNetworkBytes(aesKey.IV);
                    rsaStream.Write(keyIVBuffer, 0, keyIVBuffer.Length);
                }

                aesStream = new AESStream(baseStream, aesKey);

                baseStream.ReadTimeout = oldReadTimeout;


            }
        #endregion

        #region Read and Write
            public override int Read(byte[] buffer, int offset, int size)
            {
                if (aesStream == null)
                    throw new EncryptedNetworkStreamException(EncryptedNetworkStreamException.Reason.NetworkError);

                
                return aesStream.Read(buffer, offset, size);
            }

            public override void Write(byte[] buffer, int offset, int size)
            {
                aesStream.Write(buffer, offset, size);
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
