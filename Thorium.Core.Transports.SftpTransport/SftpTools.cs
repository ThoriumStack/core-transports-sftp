using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Renci.SshNet;

namespace Thorium.Core.Transports.SftpTransport
{
   public class SFtpTools
    {
		private bool _disposed = false;

		private SftpClient _sftpClient = null;

        public SFtpTools(string host, string username, string password, int port = 22)
        {
			_sftpClient = new SftpClient(host, port, username, password);
        }

        public void SendFile(string ftpDirectory, string sourcePath)
        {
			_sftpClient.Connect();

            try
            {
                using (var fileStream = File.OpenRead(sourcePath))
                using (var ftpStream = _sftpClient.OpenWrite(string.Format("{0}/{1}", ftpDirectory, Path.GetFileName(sourcePath))))
                {
                    var buffer = new byte[8 * 1024];
                    int count;
                    while ((count = fileStream.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        ftpStream.Write(buffer, 0, count);
                    }
                }
            }
            finally
            {
				_sftpClient.Disconnect();
            }
        }

        public List<string> GetDirectoryInfo(string ftpDirectory, List<string> fileExtensions = null)
        {
            if (!_sftpClient.IsConnected)
            {
				_sftpClient.Connect();
            }

            try
            {
                var directoryInfo = _sftpClient.ListDirectory(ftpDirectory).Where(w => w.IsRegularFile).Select(s => s.Name).ToList();

                if (fileExtensions == null)
                {
                    return directoryInfo;
                }

                return fileExtensions.Count == 0 ? directoryInfo : (from file in directoryInfo from fileExtension in fileExtensions where file.Contains(fileExtension) select file).ToList();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            finally
            {
				_sftpClient.Disconnect();
            }
        }

        public bool Exists(string ftpDirectory)
        {
            if (!_sftpClient.IsConnected)
            {
                _sftpClient.Connect();
            }
            return (_sftpClient.Exists(ftpDirectory));
        }

        public void CreateDirectory(string directoryPath)
        {
            if (!_sftpClient.IsConnected)
            {
                _sftpClient.Connect();
            }
            _sftpClient.CreateDirectory(directoryPath);
        }

        public string DownloadFile(string ftpDirectory, string sourcePath)
        {
            if (!_sftpClient.IsConnected)
            {
				_sftpClient.Connect();
            }

            var filePath = Path.GetTempFileName();

            try
            {
                using (var fileStream = File.OpenWrite(filePath))
                {
					_sftpClient.DownloadFile(ftpDirectory + "/" + sourcePath, fileStream);
                }

                return filePath;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            finally
            {
				_sftpClient.Disconnect();
            }
        }

        public void SendFileViaSftp(string ftpDirectory, string sourcePath)
        {
			_sftpClient.Connect();

            try
            {
                using (var fileStream = File.OpenRead(sourcePath))
                using (var ftpStream = _sftpClient.OpenWrite(string.Format("{0}/{1}", ftpDirectory, Path.GetFileName(sourcePath))))
                {
                    var buffer = new byte[8 * 1024];
                    int count;
                    while ((count = fileStream.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        ftpStream.Write(buffer, 0, count);
                    }
                }
            }
            finally
            {
				_sftpClient.Disconnect();
            }
        }

        public void SendFile(string ftpDirectory, string filename, Stream sourceStream)
        {
            SendFile($"{ftpDirectory}/{filename}", sourceStream);
        }

        public void SendFile(string remotePath, Stream sourceStream)
        {
			_sftpClient.Connect();
            try
            {
                //using (var ftpStream = _sftpClient.OpenWrite(remotePath))
                {
                    //The copyfile method does not work for intecon
                    sourceStream.Seek(0, SeekOrigin.Begin);
                    sourceStream.Position = 0;
					_sftpClient.UploadFile(sourceStream, remotePath);
                    sourceStream.Close();
                }
            }
            finally
            {
				_sftpClient.Disconnect();
            }
        }

        public MemoryStream DownloadFileToMemory(string remoteFilePath)
        {
			_sftpClient.Connect();
            try
            {
                var downloadMem = new MemoryStream();
				_sftpClient.DownloadFile(remoteFilePath, downloadMem);
                return downloadMem;
            }
            finally
            {
				_sftpClient.Disconnect();
            }
        }

        public void RenameFile(string remoteFilePath, string newRemoteFilePath)
        {
			_sftpClient.Connect();
            try
            {
				_sftpClient.RenameFile(remoteFilePath, newRemoteFilePath);
            }
            finally
            {
				_sftpClient.Disconnect();
            }
        }
        /// <summary>
        /// List files in a remote directory. 
        /// </summary>
        /// <param name="path">The remote directory to list</param>
        /// <param name="matchPattern">Regex pattern that will return only matching filenames</param>
        /// <returns></returns>
        public List<string> ListDirectory(string path, string matchPattern = null)
        {
			_sftpClient.Connect();
            try
            {
                var files = _sftpClient.ListDirectory(path);
                return files.Where(c => matchPattern == null || Regex.IsMatch(c.FullName, matchPattern)).OrderBy(c => c.LastWriteTime).Select(c => c.FullName).ToList();
            }
            finally
            {
				_sftpClient.Disconnect();
            }
        }

        public void DeleteFile(string remoteFilePath)
        {
			_sftpClient.Connect();
            try
            {
				_sftpClient.DeleteFile(remoteFilePath);
            }
            finally
            {
				_sftpClient.Disconnect();
            }
        }
		
	}
}