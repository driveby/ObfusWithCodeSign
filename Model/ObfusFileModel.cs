using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ObfusWithSignTool
{
    public class ObfusFileModel : BindableBase
    {
        string fileName;

        public string FileName
        {
            get { return fileName; }
            set { fileName = value; OnPropertyChanged("FileName"); }
        }

        string fileFullPath;

        public string FileFullPath
        {
            get { return fileFullPath; }
            set { fileFullPath = value; OnPropertyChanged("FileFullPath"); }
        }

        string extension;

        public string Extension
        {
            get { return extension; }
            set { extension = value; OnPropertyChanged("Extension"); }
        }

        bool isNeedObfuscation;

        public bool IsNeedObfuscation
        {
            get { return isNeedObfuscation; }
            set { isNeedObfuscation = value; OnPropertyChanged("IsNeedObfuscation"); }
        }

        bool isObfuscated;

        public bool IsObfuscated
        {
            get { return isObfuscated; }
            set { isObfuscated = value; OnPropertyChanged("IsObfuscated"); }
        }

        private string codeSignMessage;

        public string CodeSignMessage
        {
            get { return codeSignMessage; }
            set { codeSignMessage = value; OnPropertyChanged("CodeSignMessage"); }
        }
    }
}
