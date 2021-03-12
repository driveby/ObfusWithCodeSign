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
            set { newVersion = value; OnPropertyChanged("NewVersion"); }
        }

        private bool isExcepted;

        public bool IsExcepted
        {
            get { return isExcepted; }
            set { isExcepted = value; OnPropertyChanged("IsExcepted"); }
        }
    }
}
