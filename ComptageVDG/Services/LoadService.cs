using ComptageVDG.Helpers;
using ComptageVDG.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace ComptageVDG.Services
{
    public class LoadService
    {
        public string FileIni;


        public FileIniModel IniModel;

        public bool FileIniExist
        {
            get { return File.Exists(FileIni); }
        }

        public LoadService()
        {
            if(!FileIniExist)
                FileIni = string.Empty;
        }

        public LoadService( string fileIni):this()
        {
            FileIni = fileIni;
            if (FileIniExist)
                ReadIni();
        }


        public  FileIniModel? ReadIni()
        {
            try
            {

               IniModel =  SerialisationHelper.DeserialiserFromFile<FileIniModel>(FileIni);
               return IniModel;               
            }
            catch
            {
                return null;
            }
        }
        
        public bool WriteIni(string fileIni = "")
        {

            try
            {
                if (!string.IsNullOrEmpty(fileIni))
                    FileIni = FileIni;

                return SerialisationHelper.SerialiserToFile(IniModel, FileIni);
            }
            catch
            {
                return false;
            }
        }
    }
}
