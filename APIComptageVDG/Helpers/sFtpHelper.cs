using System;
using System.IO;
using System.Reflection.Metadata.Ecma335;
using Renci.SshNet;
using Renci.SshNet.Sftp;

namespace APIComptageVDG.Helpers
{
    public static class sFtpHelper
    {
        public static SftpManager Instance;

        public static async Task AsyncUploadFile(string localPath)
        {
            if (Instance == null)
                return;

            await Task.Run(() =>
            {
                Instance.UploadFile(localPath);
            });
        }


        public static async Task AsyncDownLoadFile(string remoteFileName, string localPath)
        {
            if (Instance == null)
                return;

           await Task.Run(() =>
            {
                Instance.DownloadFile(remoteFileName,  localPath);
            });
        }


        public static async Task<IEnumerable<SftpFile>> ListDirectory(string remotePath)
        {
            if (Instance == null)
                return null;

           return await Task.Run(() =>
            {
               return  Instance.ListDirectory(remotePath);
            });
        }

        public static async Task Delete(string remotePath)
        {
            if (Instance == null)
                return ;

            await Task.Run(() =>
            {
                Instance.Delete(remotePath);
            });
        }

    }


    public class SftpManager
    {
        private readonly string _host;
        private readonly int _port;
        private readonly string _username;
        private readonly string _password;
        private readonly string _remotePath;

        public SftpManager(string host, int port, string username, string password, string remotePath)
        {
            _host = host;
            _port = port;
            _username = username;
            _password = password;
            _remotePath = remotePath;
        }

        public void UploadFile(string localPath)
        {
            using (var client = new SftpClient(_host, _port, _username, _password))
            {
                client.Connect();
                client.ChangeDirectory(_remotePath);

                using (var fileStream = new FileStream(localPath, FileMode.Open))
                {
                    client.UploadFile(fileStream, Path.GetFileName(localPath));
                }

                client.Disconnect();
            }
        }

        public void DownloadFile(string remoteFileName, string localPath)
        {
            using (var client = new SftpClient(_host, _port, _username, _password))
            {
                client.Connect();
                client.ChangeDirectory(_remotePath);

                using (var fileStream = new FileStream(localPath, FileMode.Create))
                {
                    client.DownloadFile(remoteFileName, fileStream);
                }

                client.Disconnect();
            }
        }

        public IEnumerable<SftpFile> ListDirectory(string remotePath)
        {
            using (var client = new SftpClient(_host, _port, _username, _password))
            {
                client.Connect();
                client.ChangeDirectory(remotePath);

                var files = client.ListDirectory(".").Where(f => !f.IsDirectory);

                client.Disconnect();

                return files;
            }
        }

        public void Delete(string remotePath)
        {
            using (var client = new SftpClient(_host, _port, _username, _password))
            {
                client.Connect();

                if (client.Exists(remotePath))
                {
                    if (client.GetAttributes(remotePath).IsDirectory)
                    {
                        client.DeleteDirectory(remotePath);
                    }
                    else
                    {
                        client.DeleteFile(remotePath);
                    }
                }

                client.Disconnect();
            }
        }

        
    }

}
