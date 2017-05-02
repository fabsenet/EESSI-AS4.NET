﻿using System;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Xml;

namespace Eu.EDelivery.AS4.VolumeTests
{
    /// <summary>
    /// Facade for the AS4 Component as a Corner.
    /// </summary>
    public class Corner : IDisposable
    {
        private readonly Process _cornerProcess;
        private readonly DirectoryInfo _cornerDirectory;

        /// <summary>
        /// Initializes a new instance of the <see cref="Corner"/> class.
        /// </summary>
        /// <param name="cornerDirectory"></param>
        /// <param name="cornerProcess"></param>
        private Corner(DirectoryInfo cornerDirectory, Process cornerProcess)
        {
            _cornerProcess = cornerProcess;
            _cornerDirectory = cornerDirectory;
        }

        /// <summary>
        /// Place the given <paramref name="messageContents"/> <paramref name="messageCount"/> times at the Corner's location to retrieve files.
        /// </summary>
        /// <param name="messageCount">Amount of messages to send.</param>
        /// <param name="messageContents">Content of the message to send.</param>
        public void PlaceMessages(int messageCount, string messageContents)
        {
            for (var i = 0; i < messageCount; i++)
            {
                string id = Guid.NewGuid().ToString();
                string generatedMessage = messageContents.Replace("__ATTACHMENTID__", id);
                string outMessagePath = Path.Combine(_cornerDirectory.FullName, $@"messages\out\{id}.xml");

                File.WriteAllText(outMessagePath, generatedMessage);
            }
        }

        /// <summary>
        /// Count the messages that are delivered on the created corner.
        /// </summary>
        /// <param name="searchPattern">
        /// The search string to match against the names of files. This parameter can contain a combination of valid literal path
        /// and wildcard (* and ?) characters (see Remarks), but doesn't support regular expressions. The default pattern is "*",
        /// which returns all files.
        /// </param>
        /// <returns></returns>
        public int CountDeliveredMessages(string searchPattern = "*")
        {
            DirectoryInfo deliverDirectory = GetMessageDirectory(subDirectory: "in");
            return deliverDirectory.GetFiles(searchPattern).Length;
        }

        /// <summary>
        /// Cleanup the delivered messages from the Corner's deliver directory.
        /// </summary>
        public void CleanupMessages()
        {
           CleanUpMessageDirectory("in");
           CleanUpMessageDirectory("out");
        }

        private void CleanUpMessageDirectory(string subDirectory)
        {
            DirectoryInfo messageDirectory = GetMessageDirectory(subDirectory);

            foreach (FileInfo deliveredMessage in messageDirectory.GetFiles())
            {
                deliveredMessage.Delete();
            }
        }

        private DirectoryInfo GetMessageDirectory(string subDirectory)
        {
            string deliverPath = Path.Combine(_cornerDirectory.FullName, "messages", subDirectory);
            return new DirectoryInfo(deliverPath);
        }

        /// <summary>
        /// Create a new instance of the <see cref="Corner"/> instance.
        /// </summary>
        /// <param name="prefix">Corner Prefix</param>
        /// <returns></returns>
        public static Corner StartNew(string prefix)
        {
            DirectoryInfo cornerDirectory = SetupCornerFixture(prefix);

            var cornerInfo =
                new ProcessStartInfo(
                    Path.Combine(cornerDirectory.FullName, "Eu.EDelivery.AS4.ServiceHandler.ConsoleHost.exe"));
            cornerInfo.WorkingDirectory = cornerDirectory.FullName;
           
            var corner = new Corner(cornerDirectory, Process.Start(cornerInfo));
            Thread.Sleep(1000);

            return corner;
        }

        private static DirectoryInfo SetupCornerFixture(string cornerPrefix)
        {
            var outputDirectory = new DirectoryInfo(Directory.GetCurrentDirectory());
            DirectoryInfo cornerDirectory = CreateCornerIn(outputDirectory, $"output-{cornerPrefix}");

            CopyDirectory(outputDirectory, cornerDirectory.FullName);
            IncludeCornerSettingsIn(cornerDirectory, $"{cornerPrefix}-settings.xml");
            IncludeCornerPModesIn(cornerDirectory);

            return cornerDirectory;
        }

        private static DirectoryInfo CreateCornerIn(FileSystemInfo outputDirectory, string corner)
        {
            return Directory.CreateDirectory(outputDirectory.FullName.Replace("output", corner));
        }

        private static void CopyDirectory(DirectoryInfo sourceDirectory, string destDirName)
        {
            if (!Directory.Exists(destDirName))
            {
                Directory.CreateDirectory(destDirName);
            }

            CopyFiles(sourceDirectory, destDirName);
            CopySubDirectories(sourceDirectory, destDirName);
        }

        private static void CopyFiles(DirectoryInfo sourceDirectory, string destDirName)
        {
            FileInfo[] files = sourceDirectory.GetFiles();
            foreach (FileInfo file in files)
            {
                string temppath = Path.Combine(destDirName, file.Name);
                file.CopyTo(temppath, overwrite: true);
            }
        }

        private static void CopySubDirectories(DirectoryInfo sourceDirectory, string destDirName)
        {
            DirectoryInfo[] directories = sourceDirectory.GetDirectories();
            foreach (DirectoryInfo subDirectory in directories)
            {
                string temppath = Path.Combine(destDirName, subDirectory.Name);
                CopyDirectory(subDirectory, temppath);
            }
        }

        private static void IncludeCornerSettingsIn(DirectoryInfo cornerDirectory, string cornerSettingsFileName)
        {
            FileInfo cornerSettings =
                cornerDirectory.GetFiles("*.xml", SearchOption.AllDirectories)
                               .First(f => f.Name.Equals(cornerSettingsFileName));

            string newSettingsFilePath = Path.Combine(cornerDirectory.FullName, "config", "settings.xml");
            File.Copy(cornerSettings.FullName, newSettingsFilePath, overwrite: true);

            DropSettingsDatabase(newSettingsFilePath);
        }

        private static void DropSettingsDatabase(string newSettingsFilePath)
        {
            string xml = File.ReadAllText(newSettingsFilePath);
            var xmlDocument = new XmlDocument();
            xmlDocument.LoadXml(xml);

            string connectionString = xmlDocument.SelectSingleNode("//*[local-name()='ConnectionString']").InnerText;
            using (var sqlConnection = new SqlConnection(connectionString))
            {
                sqlConnection.Open();
                
                var dropDatabaseCommand = new SqlCommand($"USE [master]; DROP DATABASE {sqlConnection.Database}", sqlConnection);
                dropDatabaseCommand.ExecuteNonQuery();
            }
        }

        private static void IncludeCornerPModesIn(FileSystemInfo cornerDirectory)
        {
            Func<string, DirectoryInfo> getPModeDirectory =
                subFolder => new DirectoryInfo(Path.Combine(cornerDirectory.FullName, "config", subFolder));

            DirectoryInfo volumeSendPModes = getPModeDirectory(@"volumetest-pmodes\send-pmodes"),
                          volumeReceivePModes = getPModeDirectory(@"volumetest-pmodes\receive-pmodes");

            DirectoryInfo outputSendPModes = getPModeDirectory(@"send-pmodes"),
                          outputReceivePModes = getPModeDirectory(@"receive-pmodes");

            CopyFiles(volumeSendPModes, outputSendPModes.FullName);
            CopyFiles(volumeReceivePModes, outputReceivePModes.FullName);
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            if (!_cornerProcess.HasExited)
            {
                _cornerProcess.Kill();
            }
        }
    }
}