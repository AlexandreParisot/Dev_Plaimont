using System;
using System.IO;
using System.Reflection.Metadata.Ecma335;
using Renci.SshNet;
using Renci.SshNet.Sftp;

namespace APIComptageVDG.Helpers
{
    public static class sFtpHelper
    {
        private static SftpManager Instance = null;
        

        public static bool SetInstance(string host, string username, string password, string remotePath, int port = 22)
        {

            Instance = new SftpManager(host, port, username, password, remotePath);

            if (Instance == null)
                return false;
            return true;
        }

        public static async Task<bool> AsyncUploadFile(string localPath)
        {
            if (Instance == null)
                return false;

          return await Task.Run(() =>
            {
               return  Instance.UploadFile(localPath);
            });
        }


        public static async Task<bool> AsyncDownLoadFile(string remoteFileName, string localPath)
        {
            if (Instance == null)
                return false;

           return  await Task.Run(() =>
            {
               return  Instance.DownloadFile(remoteFileName,  localPath);
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

        public static async Task<bool> Delete(string remotePath)
        {
            if (Instance == null)
                return false ;

           return  await Task.Run(() =>
            {
               return  Instance.Delete(remotePath);
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

        public bool UploadFile(string localPath)
        {
            try
            {
                var client = new SftpClient(_host, _port, _username, _password);
                client.Connect();
                client.ChangeDirectory(_remotePath);

                using (var fileStream = new FileStream(localPath, FileMode.Open))
                {
                    client.UploadFile(fileStream, Path.GetFileName(localPath));
                }

                client.Disconnect();
                return true;
            }catch(Exception ex)
            {
                Gestion.Erreur($"Upload File Sftp : {localPath} - {ex.Message}");
                return false;
            }
            
        }

        public bool DownloadFile(string remoteFileName, string localPath)
        {
          try{
                var client = new SftpClient(_host, _port, _username, _password);
                client.Connect();
                client.ChangeDirectory(_remotePath);

                using (var fileStream = new FileStream(localPath, FileMode.Create))
                {
                    client.DownloadFile(remoteFileName, fileStream);
                }

                client.Disconnect();
                return true;
            }
            catch(Exception ex)
            {
            Gestion.Erreur($"Download File Sftp local : {localPath} / remot : {remoteFileName} - {ex.Message}");
            return false;
            }
        }

        public IEnumerable<SftpFile> ListDirectory(string remotePath)
        {
            try
            {
                var client = new SftpClient(_host, _port, _username, _password);
                client.Connect();
                client.ChangeDirectory(remotePath);

                var files = client.ListDirectory(".").Where(f => !f.IsDirectory);

                client.Disconnect();

                return files;
            }catch(Exception ex)
            {
                Gestion.Erreur($"Remote Sftp Path : {remotePath} - {ex.Message} ");
                return null;
            }
        }

        public bool Delete(string remotePath)
        {

            try{
                var client = new SftpClient(_host, _port, _username, _password);
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

                return true;
            }catch(Exception ex)
            {
                Gestion.Erreur($"File Remote delete : {remotePath} - {ex.Message}");
                return false;
            }
        }

        
    }

}
