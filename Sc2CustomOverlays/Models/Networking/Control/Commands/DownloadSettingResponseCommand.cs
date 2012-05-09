using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Sc2CustomOverlays.Models.Networking.Control.Commands
{
    public class DownloadSettingResponseCommand : ControlCommand
    {
        #region Singleton Requirements
            protected static DownloadSettingResponseCommand _instance = new DownloadSettingResponseCommand();
            public static DownloadSettingResponseCommand Instance { get { return _instance; } }

            protected DownloadSettingResponseCommand()
            {

            }
        #endregion

        // Returns CommandResult (see below) that holds success/failure and implementation specific data.
        //  Return Data: 1
        //   DirectoryInfo savedDirectory
        public override CommandResult HandleCommand(Stream ns)
        {
            Dictionary<string, object> returnData = new Dictionary<string,object>();
            DirectoryInfo tempDirectory = OverlaySettings.OverlaysTempBasePath;

            DirectoryInfo savedDirectory = null;
            try
            {
                savedDirectory = ReceiveDirectory(ns, tempDirectory);
            } catch (Exception) {
                return new CommandResult(false);
            }

            returnData["savedDirectory"] = savedDirectory;
            return new CommandResult(true, returnData);
        }

        // Returns whether the command sent successfully or not.
        //  In Parameters: 1
        //   DirectoryInfo fromDirectory
        public override bool SendCommand(Stream ns, Dictionary<string, object> parameters = null)
        {
            // Validate that parameters is valid.
            if (parameters == null)
                throw new CommandException("DownloadSettingResponseCommand", true, CommandException.Reason.MissingParameter);

            DirectoryInfo fromDirectory = null;

            // Load parameters into local variable
            try
            {
                fromDirectory = (DirectoryInfo) parameters["fromDirectory"];
            } catch (KeyNotFoundException) {
                throw new CommandException("DownloadSettingResponseCommand", true, CommandException.Reason.MissingParameter);
            } catch (InvalidCastException) {
                throw new CommandException("DownloadSettingResponseCommand", true, CommandException.Reason.InvalidParameterType);
            }

            // Go ahead and write out the command byte since this is a large packet, we will use the network layer to buffer the output, instead of loading all data into memory.
            try
            {
                ns.Write(new byte[1] { (byte) ControlCommand.Command.DownloadSettingResponse }, 0, 1);
            } catch (Exception) {
                return false;
            }

            try
            {
                SendDirectory(fromDirectory, ns);
            } catch (Exception) {
                return false;
            }

            return true;
        }

        private void SendDirectory(DirectoryInfo currentDirectory, Stream ns)
        {
            // Send bytes of Directory name
            byte[] directoryNameBytes = SerializedString.ToNetworkBytes(currentDirectory.Name);
            ns.Write(directoryNameBytes, 0, directoryNameBytes.Length);

            
            FileInfo[] files = currentDirectory.GetFiles();
            DirectoryInfo[] directories = currentDirectory.GetDirectories();

            int filesCount = files.Length;
            int directoriesCount = directories.Length;

            // Send bytes of filesCount and directoriesCount
            ns.Write(BitConverter.GetBytes(filesCount), 0, 4);
            ns.Write(BitConverter.GetBytes(directoriesCount), 0, 4);

            for (int x = 0; x < filesCount; ++x)
            {
                FileStream fs = files[x].OpenRead();

                // Send bytes of file name
                byte[] fileNameBytes = SerializedString.ToNetworkBytes(files[x].Name);
                ns.Write(fileNameBytes, 0, fileNameBytes.Length);

                // Send bytes of file length (long = Int64)
                ns.Write(BitConverter.GetBytes(fs.Length), 0, 8);
                
                int bytesRead = 0;
                byte[] fileBytes = new byte[8192];

                do
                {
                    // Read in 8kB chunks
                    bytesRead = fs.Read(fileBytes, 0, 8192);

                    // Send in 8kB chunks
                    if (bytesRead > 0)
                        ns.Write(fileBytes, 0, bytesRead);

                } while (bytesRead != 0);

                fs.Close();

            }

            for (int x = 0; x < directoriesCount; ++x)
                SendDirectory(directories[x], ns);

        }

        private DirectoryInfo ReceiveDirectory(Stream ns, DirectoryInfo parentDirectory)
        {
            // Create subdirectory, remove all \, /, and ..'s to preserve security
            string directoryName = SerializedString.FromNetworkBytes(ns).Replace("\\", "").Replace("/", "").Replace("..", "");
            
            
            DirectoryInfo receivedDirectory = parentDirectory.CreateSubdirectory(directoryName);

            byte[] fileCountBytes = new byte[4];
            byte[] directoryCountBytes = new byte[4];

            StreamHelper.ForceReadAll(ns, fileCountBytes, 0, 4);
            StreamHelper.ForceReadAll(ns, directoryCountBytes, 0, 4);

            int fileCount = BitConverter.ToInt32(fileCountBytes, 0);
            int directoryCount = BitConverter.ToInt32(directoryCountBytes, 0);

            // Receive files
            for (int x = 0; x < fileCount; ++x)
            {
                string fileName = SerializedString.FromNetworkBytes(ns).Replace("\\", "").Replace("/", "");
                FileInfo receivedFile = new FileInfo(Path.Combine(receivedDirectory.FullName, fileName));

                byte[] fileSizeBytes = new byte[8];
                StreamHelper.ForceReadAll(ns, fileSizeBytes, 0, 8);

                long fileSize = BitConverter.ToInt64(fileSizeBytes, 0);

                // If it exists, try to delete and start new
                try
                {
                    if (receivedFile.Exists)
                        receivedFile.Delete();
                } catch (Exception) {

                }

                FileStream fs = null;
                try
                {
                    fs = receivedFile.OpenWrite();
                } catch (Exception) {
                    // Unable to open file for write
                }

                long remainingBytes = fileSize;

                byte[] fileBytes = new byte[8192];

                while (remainingBytes > 0)
                {
                    // Read from network and write to file
                    int chunkSize = (remainingBytes > 8192 ? 8192 : (int) remainingBytes);
                    StreamHelper.ForceReadAll(ns, fileBytes, 0, chunkSize);
                    if (fs != null)
                        fs.Write(fileBytes, 0, chunkSize);

                    // Subtract out bytes just received
                    remainingBytes -= chunkSize;
                }

                if (fs != null)
                    fs.Close();

            }

            // Receive directory
            for (int x = 0; x < directoryCount; ++x)
                ReceiveDirectory(ns, receivedDirectory);

            return receivedDirectory;
        }

    }



}
