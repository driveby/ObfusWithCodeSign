using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ObfusWithSignTool
{
    public class AssemblyVersionControlWindowViewModel : BindableBase
    {
        public AssemblyVersionControlWindowViewModel()
        {
            assemblyList = new ObservableCollection<AssemblyInfoModel>();
        }

        public void Loaded()
        {

        }

        public void Unloaded()
        {

        }

        public AssemblyVersionControlWindow WindowInstance { get; set; }

        private string solutionFolderPath;

        public string SolutionFolderPath
        {
            get { return solutionFolderPath; }
            set { solutionFolderPath = value; OnPropertyChanged("SolutionFolderPath"); }
        }

        private string batchVersion;

        public string BatchVersion
        {
            get { return batchVersion; }
            set { batchVersion = value; OnPropertyChanged("BatchVersion"); }
        }

        private ObservableCollection<AssemblyInfoModel> assemblyList;

        public ObservableCollection<AssemblyInfoModel> AssemblyList
        {
            get { return assemblyList; }
            set { assemblyList = value; OnPropertyChanged("AssemblyList"); }
        }

        private DelegateCommand openSolutionFolderCommand;

        public DelegateCommand OpenSolutionFolderCommand
        {
            get
            {
                if (openSolutionFolderCommand == null)
                    openSolutionFolderCommand = new DelegateCommand(ExecuteOpenSolutionFolder);
                return openSolutionFolderCommand;
            }
        }

        private void ExecuteOpenSolutionFolder()
        {
            var folderDialog = new System.Windows.Forms.FolderBrowserDialog();
            folderDialog.SelectedPath = System.IO.Directory.GetCurrentDirectory();

            if (System.Windows.Forms.DialogResult.OK != folderDialog.ShowDialog()) return;

            this.SolutionFolderPath = folderDialog.SelectedPath;

            RefreshAssemblyList();
        }

        private DelegateCommand refreshCommand;

        public DelegateCommand RefreshCommand
        {
            get
            {
                if (refreshCommand == null)
                    refreshCommand = new DelegateCommand(ExecuteRefresh);
                return refreshCommand; 
            }
        }

        private void ExecuteRefresh()
        {
            if (assemblyList != null)
            {
                this.AssemblyList.Clear(); 
            }

            RefreshAssemblyList();
        }

        private DelegateCommand batchVersionSaveCommand;
        /// <summary>
        /// 버전 일괄 부여
        /// </summary>
        public DelegateCommand BatchVersionSaveCommand
        {
            get
            {
                if (batchVersionSaveCommand == null)
                    batchVersionSaveCommand = new DelegateCommand(ExecuteBatchVersionSave);
                return batchVersionSaveCommand;
            }
        }

        private void ExecuteBatchVersionSave()
        {
            if (batchVersion.IsNullOrEmpty()) return;
            if (assemblyList == null || assemblyList.Count == 0) return;

            foreach(var assembly in assemblyList)
            {
                if (true == assembly.IsExcepted || assembly.AssemblyVersion == "1.0.0.0") continue;
                if (assembly.NewVersion == batchVersion) continue;

                assembly.NewVersion = batchVersion;

                string filePath = Path.Combine(assembly.ProjectFolderPath, @"Properties\AssemblyInfo.cs");

                if (!File.Exists(filePath))
                {
                    return;
                }

                StreamReader reader = new StreamReader(filePath);
                StreamWriter writer = new StreamWriter(filePath + ".out");
                String line;

                while ((line = reader.ReadLine()) != null)
                {
                    line = ProcessLine(assembly, line);
                    writer.WriteLine(line);
                }
                reader.Close();
                reader.Dispose();
                writer.Close();
                writer.Dispose();

                File.Delete(filePath);
                File.Move(filePath + ".out", filePath);
            }
        }

        private DelegateCommand<AssemblyInfoModel> itemVersionSaveCommand;
        /// <summary>
        /// 개별항목 버전 설정
        /// </summary>
        public DelegateCommand<AssemblyInfoModel> ItemVersionSaveCommand
        {
            get
            {
                if (itemVersionSaveCommand == null)
                    itemVersionSaveCommand = new DelegateCommand<AssemblyInfoModel>(ExecuteItemVersionSave);
                return itemVersionSaveCommand;
            }
        }

        private void ExecuteItemVersionSave(AssemblyInfoModel param)
        {
            if (param == null) return;
            if (param.AssemblyVersion == param.NewVersion) return;

            string filePath = Path.Combine(param.ProjectFolderPath, @"Properties\AssemblyInfo.cs");

            if (!File.Exists(filePath))
            {
                return;
            }

            StreamReader reader = new StreamReader(filePath);
            StreamWriter writer = new StreamWriter(filePath + ".out");
            String line;

            while ((line = reader.ReadLine()) != null)
            {
                line = ProcessLine(param, line);
                writer.WriteLine(line);
            }
            reader.Close();
            reader.Dispose();
            writer.Close();
            writer.Dispose();

            File.Delete(filePath);
            File.Move(filePath + ".out", filePath);
        }

        private DelegateCommand<AssemblyInfoModel> exceptItemCommand;

        public DelegateCommand<AssemblyInfoModel> ExceptItemCommand
        {
            get
            {
                if (exceptItemCommand == null)
                    exceptItemCommand = new DelegateCommand<AssemblyInfoModel>(ExecuteExceptItem);
                return exceptItemCommand; 
            }
        }

        private void ExecuteExceptItem(AssemblyInfoModel param)
        {
            if (param == null) return;

            param.IsExcepted = !param.IsExcepted;
        }

        private void ExtractLine(AssemblyInfoModel item, string line)
        {
            ExtractVersion(item, line, "[assembly: AssemblyTitle(\"", 0);
            ExtractVersion(item, line, "[assembly: AssemblyVersion(\"", 1);
            ExtractVersion(item, line, "[assembly: AssemblyFileVersion(\"", 2);
        }

        private void ExtractVersion(AssemblyInfoModel item, string line, string part, int type)
        {
            int spos = line.IndexOf(part);
            if (spos >= 0)
            {
                spos += part.Length;
                int epos = line.IndexOf('"', spos);
                string oldValue = line.Substring(spos, epos - spos);

                if (0 == type)
                {
                    item.AssemblyName = oldValue;
                }
                else if (1 == type)
                {
                    item.AssemblyVersion = oldValue;
                    item.NewVersion = oldValue;
                }
                else if(2 == type)
                {
                    item.FileVersion = oldValue;
                    item.NewVersion = oldValue;
                }
            }
        }

        private string ProcessLine(AssemblyInfoModel item, string line)
        {
            line = ProcessLinePart(item, line, "[assembly: AssemblyVersion(\"");
            line = ProcessLinePart(item, line, "[assembly: AssemblyFileVersion(\"");

            return line;
        }

        private string ProcessLinePart(AssemblyInfoModel item, string line, string part)
        {
            int spos = line.IndexOf(part);
            if (spos >= 0)
            {
                spos += part.Length;
                int epos = line.IndexOf('"', spos);

                StringBuilder str = new StringBuilder(line);
                str.Remove(spos, epos - spos);
                str.Insert(spos, item.NewVersion);
                line = str.ToString();
            }

            return line;
        }

        private void RefreshAssemblyList()
        {
            if (solutionFolderPath.IsNullOrEmpty()) return;

            DirectoryInfo solutionFolder = new DirectoryInfo(solutionFolderPath);

            var directories = solutionFolder.GetDirectories("*.*", SearchOption.AllDirectories);

            List<FileInfo> files = new List<FileInfo>();

            foreach (var directory in directories)
            {
                files.AddRange(directory.GetFiles("*.csproj", SearchOption.TopDirectoryOnly));
            }

            if (files.Count != 0)
            {
                List<AssemblyInfoModel> list = new List<AssemblyInfoModel>();

                foreach (var file in files)
                {
                    var assemInfo = new AssemblyInfoModel();
                    assemInfo.AssemblyName = Path.GetFileName(file.FullName);
                    var directory = Directory.GetParent(file.FullName);
                    assemInfo.ProjectFolderPath = directory.FullName;

                    list.Add(assemInfo);
                }

                if (list.Count != 0)
                {
                    foreach (var assembly in list)
                    {
                        var assemblyPath = Path.Combine(assembly.ProjectFolderPath, @"Properties\AssemblyInfo.cs");
                        if (!File.Exists(assemblyPath))
                        {
                            return;
                        }

                        StreamReader reader = new StreamReader(assemblyPath);
                        String line;

                        while ((line = reader.ReadLine()) != null)
                        {
                            ExtractLine(assembly, line);
                        }
                        reader.Close();
                    }

                    this.AssemblyList = new ObservableCollection<AssemblyInfoModel>(list.OrderBy(o => o.AssemblyName));
                }
            }
        }
    }
}
