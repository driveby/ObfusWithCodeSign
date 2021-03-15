using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ObfusWithSignTool
{
    public class AssemblyInfoModel : BindableBase
    {
        private string assemblyName;
        /// <summary>
        /// Project file name
        /// eg. AccountBookSpace.csproj
        /// </summary>
        public string AssemblyName
        {
            get { return assemblyName; }
            set { assemblyName = value; OnPropertyChanged("AssemblyName"); }
        }

        private string projectFolderPath;
        /// <summary>
        /// Project root folder
        /// eg. D:\SplitBranch\Client\1. Workspace\AccountBookSpace
        /// </summary>
        public string ProjectFolderPath
        {
            get { return projectFolderPath; }
            set { projectFolderPath = value; OnPropertyChanged("ProjectFolderPath"); }
        }

        private string assemblyVersion;

        public string AssemblyVersion
        {
            get { return assemblyVersion; }
            set { assemblyVersion = value; OnPropertyChanged("AssemblyVersion"); }
        }

        private string fileVersion;

        public string FileVersion
        {
            get { return fileVersion; }
            set { fileVersion = value; OnPropertyChanged("FileVersion"); }
        }

        private string newVersion;

        public string NewVersion
        {
            get { return newVersion; }
            set 
            { 
                newVersion = value; OnPropertyChanged("NewVersion"); 

                if(value.IsNotNullOrEmpty() && value.Contains("."))
                {
                    var splited = value.Split(new char[] { '.' });
                    if(splited != null && splited.Count() == 4)
                    {
                        NewVersionPart1 = splited[0];
                        NewVersionPart2 = splited[1];
                        NewVersionPart3 = splited[2];
                        NewVersionPart4 = splited[3];
                    }
                }
            }
        }

        private string newVersionPart1;

        public string NewVersionPart1
        {
            get { return newVersionPart1; }
            set { newVersionPart1 = value; OnPropertyChanged("NewVersionPart1"); }
        }

        private string newVersionPart2;

        public string NewVersionPart2
        {
            get { return newVersionPart2; }
            set { newVersionPart2 = value; OnPropertyChanged("NewVersionPart2"); }
        }

        private string newVersionPart3;

        public string NewVersionPart3
        {
            get { return newVersionPart3; }
            set { newVersionPart3 = value; OnPropertyChanged("NewVersionPart3"); }
        }

        private string newVersionPart4;

        public string NewVersionPart4
        {
            get { return newVersionPart4; }
            set { newVersionPart4 = value; OnPropertyChanged("NewVersionPart4"); }
        }

        private bool isExcepted;

        public bool IsExcepted
        {
            get { return isExcepted; }
            set { isExcepted = value; OnPropertyChanged("IsExcepted"); }
        }
    }
}
