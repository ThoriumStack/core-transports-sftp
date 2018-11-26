using System;
using System.Collections.Generic;
using System.IO;
using MyBucks.Core.DataIntegration.Interfaces;

namespace Thorium.Core.Transports.SftpTransport
{
 public class SftpTransport : IFileIntegrationTransport
    {
        private MemoryStream _rawData;
        public string FilePath { get; set; }

        public string Password { get; set; }
        public string Username { get; set; }
        public string Hostname { get; set; }
        /// <summary>
        /// Prot number. Defaults to 22
        /// </summary>
        public int Port { get; set; } = 22;

        public MemoryStream CollectRawData()
        {
            var tools = GetSftpConnection();
            _rawData = tools.DownloadFileToMemory(FilePath);
            return tools.DownloadFileToMemory(FilePath);
        }

        (bool, string) IIntegrationTransport.SendData(MemoryStream rawData)
        {
            try
            {
                var tools = GetSftpConnection();
                tools.SendFile(FilePath, rawData);
                AfterSend?.Invoke();
            }
            catch (Exception e)
            {
                return (false, e.Message);
            }
            return (true, $"Successfully transferred file '{FilePath}' via sftp");
        }


        public Action AfterSend { get; set; }
        public Action<Exception> ErrorAction { get; set; }

        public string PathSeperator => "/";

        string IFileIntegrationTransport.CurrentTransportMethod => "sftp";

        public void RenameFile(string newPath)
        {
            var tools = GetSftpConnection();
            tools.RenameFile(FilePath, newPath);
        }

        public MemoryStream GetLastRawData()
        {
            return _rawData;
        }

        private SFtpTools GetSftpConnection()
        {
            return new SFtpTools(Hostname, Username, Password, Port);
        }

        public List<string> ListFiles(string directory=".", string matchExpression=null)
        {
            var tools = GetSftpConnection();
            return tools.ListDirectory(directory, matchExpression);
        }

        public void DeleteFile()
        {
            var tools = GetSftpConnection();
            tools.DeleteFile(FilePath);
        }

        public void EnsureDirectory(string directoryPath)
        {
            var tools = GetSftpConnection();
            if (!tools.Exists(directoryPath))
            {
                tools.CreateDirectory(directoryPath);
            }
        }

        public bool FileExists(string targetFileNameAndPath)
        {
            var tools = GetSftpConnection();
            return tools.Exists(targetFileNameAndPath);
        }
    }
}