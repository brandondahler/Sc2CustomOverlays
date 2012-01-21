using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.IO;
using System.Security;
using System.Security.Cryptography;
using System.Threading;

namespace Sc2CustomOverlays.Code.Networking.Encryption
{
    public sealed class EncryptedNetworkStream : NetworkStream
    {
        private static RNGCryptoServiceProvider r = new RNGCryptoServiceProvider();
        
        private static uint currentId = 0;
        private uint _id = 0;
        public uint Id { get { return _id; } }
        
        private byte[] readHash = null;
        private int readHashLocation = 0;

        private byte[] writeHash = null;
        private int writeHashLocation = 0;

        #region Encryption Initialization

            #region Constructors and Initialize
                public EncryptedNetworkStream(Socket socket, string password) 
                    : base(socket) 
                { 
                    Initialize(password); 
                }

                public EncryptedNetworkStream(Socket socket, bool ownsSocket, string password) 
                    : base(socket, ownsSocket) 
                { 
                    Initialize(password); 
                }

                public EncryptedNetworkStream(Socket socket, FileAccess access, string password)
                    : base(socket, access)
                {
                    Initialize(password);
                }

                public EncryptedNetworkStream(Socket socket, FileAccess access, bool ownsSocket, string password)
                    : base(socket, access, ownsSocket)
                {
                    Initialize(password);
                }

                private void Initialize(string password)
                {
                    // Set our unique id
                    _id = currentId++;
                    NegotiateHashes(System.Text.Encoding.UTF8.GetBytes(password));
                }
            #endregion

            private void NegotiateHashes(byte[] streamPassword)
            {
                if (readHash != null || writeHash != null)
                    throw new InvalidOperationException("EncryptedNetworkStream :: Cannot negotiate hashes at this time.");

                int oldReadTimeout = ReadTimeout;

                ReadTimeout = 5000;

                /*
                * Generate and send write hash 
                */
                {

                    byte[] writeHashSizeByte = new byte[1];
                    r.GetBytes(writeHashSizeByte);
                    int writeHashSize = (int) writeHashSizeByte[0] | 256;

                    // Generate sendHash first to make extra bytes
                    byte[] sendHash = new byte[511];
                    r.GetBytes(sendHash);

                    // Copy first writeHashSize bytes from sendHash to writeHash
                    writeHash = new byte[writeHashSize];
                    Array.Copy(sendHash, writeHash, writeHashSize);

                    // Generate the data we're going to send
                    byte[] buffer = new byte[1 + 511];

                    // Fill buffer with data
                    //   Cut off MSB of writeHashSize same as (writeHashSize - 256)
                    buffer[0] = (byte)(writeHashSize & 255);
                    Array.Copy(sendHash, 0, buffer, 1, 511);

                    // XOr our password over buffer to encrypt it once with our password, 
                    //  this data can be decrypted by XOring with the same password again
                    for (int x = 0; x < 512; ++x)
                        buffer[x] = (byte)(buffer[x] ^ streamPassword[x % streamPassword.Length]);

                    base.Write(buffer, 0, 512);
                }

                /*
                * Receive and decrypt read hash
                */
                {
                    // Read 512 bytes off the line
                    byte[] readBuffer = new byte[512];

                    int totalRead = 0;
                    int curRead = 0;

                    do
                    {
                        try
                        {
                            curRead = base.Read(readBuffer, totalRead, 512 - totalRead);
                        } catch (IOException) {
                            throw new EncryptedNetworkStreamException(EncryptedNetworkStreamException.Reason.ReadTimeout);
                        }
                        totalRead += curRead;
                    } while (totalRead < 512 && curRead != 0);
                    if (curRead == 0)
                        throw new EncryptedNetworkStreamException(EncryptedNetworkStreamException.Reason.NetworkError);

                    // Decrypt the data with our password by XOring over the data
                    for (int x = 0; x < 512; ++x)
                        readBuffer[x] = (byte)(readBuffer[x] ^ streamPassword[x % streamPassword.Length]);


                    // Fill readHash
                    int readHashSize = ((int)readBuffer[0] | 256);
                    readHash = new byte[readHashSize];
                    Array.Copy(readBuffer, 1, readHash, 0, readHashSize);
                }

                /*
                    * Verify we are on the same page by hashing the readHash over itself 16384 times.
                    * Rationale is that while a single sha256 computation is fast itself, it significantly slows 
                    *   down brute force attempts by requiring it to be done so many times per key to test.
                    */
                {
                    // Hash the readHash 16,384 times over itself
                    SHA256 sha256Engine = SHA256.Create();
                    byte[] readSha256 = readHash;

                    // readSha256 should be a 32 byte array afterwards
                    for (int x = 0; x < 16384; ++x)
                        readSha256 = sha256Engine.ComputeHash(readSha256);

                    Encrypt(readSha256, 0, 32);
                    base.Write(readSha256, 0, 32);

                }

                /*
                * Make sure they got our key right
                */
                {
                    // Calculate our writeHash (what they sent)
                    SHA256 sha256Engine = SHA256.Create();
                    byte[] writeSha256 = writeHash;

                    // readSha256 should be a 32 byte array afterwards
                    for (int x = 0; x < 16384; ++x)
                        writeSha256 = sha256Engine.ComputeHash(writeSha256);

                    // Read 32 bytes off the line
                    byte[] readBuffer = new byte[32];

                    int totalRead = 0;
                    int curRead = 0;

                    do
                    {
                        try
                        {
                            curRead = base.Read(readBuffer, totalRead, 32 - totalRead);
                        } catch (IOException) {
                            throw new EncryptedNetworkStreamException(EncryptedNetworkStreamException.Reason.ReadTimeout);
                        }
                        totalRead += curRead;
                    } while (totalRead < 32 && curRead != 0);
                    if (curRead == 0)
                        throw new EncryptedNetworkStreamException(EncryptedNetworkStreamException.Reason.NetworkError);

                    // Decrypt received bytes
                    Decrypt(readBuffer, 0, 32);

                    // Ensure the authentication is correct, else throw an invalid authentication exception
                    for (int x = 0; x < 32; ++x)
                    {
                        if (readBuffer[x] != writeSha256[x])
                            throw new EncryptedNetworkStreamException(EncryptedNetworkStreamException.Reason.InvalidAuthentication);
                    }


                }

                ReadTimeout = oldReadTimeout;


            }
        #endregion

        #region Read, Write, Close

            public override int Read(byte[] buffer, int offset, int size)
            {

                int bytesRead = base.Read(buffer, offset, size);
                Decrypt(buffer, offset, size);

                return bytesRead;
            }

            public int ForceReadAll(byte[] buffer, int offset, int size)
            {
                if (size == 0) return 0;

                int totalRead = 0;
                int curRead = 0;

                do
                {
                    curRead = Read(buffer, offset + totalRead, size - totalRead);
                    totalRead += curRead;
                } while (totalRead < size && curRead != 0);
                if (curRead == 0) throw new Exception("EncryptedNetworkString:: Unable to force read all.");

                return totalRead;
            }

            public override void Write(byte[] buffer, int offset, int size)
            {
                byte[] encryptedBuffer = new byte[size];
                Array.Copy(buffer, offset, encryptedBuffer, 0, size);

                Encrypt(encryptedBuffer, 0, size);

                base.Write(encryptedBuffer, 0, size);
            }

        #endregion

        #region Encryption and Decryption

            private void Encrypt(byte[] unencryptedBuffer, int offset, int size)
            {
                for (int x = 0; x < size; ++x, ++writeHashLocation)
                {
                    writeHashLocation %= writeHash.Length;
                    unencryptedBuffer[x] = (byte)(unencryptedBuffer[x] ^ writeHash[writeHashLocation]);
                }

            }

            private void Decrypt(byte[] encryptedBuffer, int offset, int size)
            {
                for (int x = 0; x < size; ++x, ++readHashLocation)
                {
                    readHashLocation %= readHash.Length;
                    encryptedBuffer[x] = (byte)(encryptedBuffer[x] ^ readHash[readHashLocation]);
                }
            }

        #endregion

    }
}
