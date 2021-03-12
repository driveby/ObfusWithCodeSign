using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Xml.Linq;

namespace ObfusWithSignTool
{
    public class MainWindowViewModel : BindableBase, IFileDragDropTarget
    {
        public MainWindow WindowInstance { get; set; }

        private XDocument document;

        private readonly string RuleFilePath = System.IO.Directory.GetCurrentDirectory() + "\\Rule.xml";
        private readonly string BabelPath = @"C:\Program Files\Babel\babel.exe";
        private readonly string SignToolPath = @"C:\Program Files (x86)\Windows Kits\8.1\bin\x64\signtool.exe";
        private readonly string CodeSignTargetPath = @"C:\CodeSign";
        private readonly string XConfigPath = @"D:\SplitBranch\Server\OneClick.Server\XConfig";
        private readonly string XHibernatePath = @"D:\SplitBranch\Server\OneClick.Server\XHibernate";

        public void Loaded()
        {
            FileList = new ObservableCollection<ObfusFileModel>();
            ruleList = new List<RuleModel>();

            if (System.IO.File.Exists(RuleFilePath))
            {
                document = XDocument.Load(RuleFilePath);

                if (document.Root != null && true == document.Root.HasElements)
                {
                    List<RuleModel> list = new List<RuleModel>();

                    foreach (var element in document.Root.Elements("Rule"))
                    {
                        list.Add(new RuleModel { ModuleName = element.Value });
                    }

                    if (list.Count != 0)
                    {
                        ruleList = new List<RuleModel>(list.OrderBy(o => o.ModuleName));
                    }
                }
            }
        }

        public void Unloaded()
        {
            this.document = null;
        }

        private string rootFolder;

        public string RootFolder
        {
            get { return rootFolder; }
            set { rootFolder = value; OnPropertyChanged("RootFolder"); }
        }

        private ObservableCollection<ObfusFileModel> fileList;

        public ObservableCollection<ObfusFileModel> FileList
        {
            get { return fileList; }
            set { fileList = value; OnPropertyChanged("FileList"); }
        }

        private List<RuleModel> ruleList;

        private DelegateCommand settingCommand;

        public DelegateCommand SettingCommand
        {
            get
            {
                if (settingCommand == null)
                    settingCommand = new DelegateCommand(ExecuteSetting);
                return settingCommand;
            }
        }

        private void ExecuteSetting()
        {
            var win = new ObfuscationSettingWindow();
            win.Owner = this.WindowInstance;
            win.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            win.ShowDialog();
        }

        private DelegateCommand obfuscationCommand;

        public DelegateCommand ObfuscationCommand
        {
            get
            {
                if (obfuscationCommand == null)
                    obfuscationCommand = new DelegateCommand(ExecuteObfuscation);
                return obfuscationCommand;
            }
        }

        private void ExecuteObfuscation()
        {
            if (fileList == null || fileList.Count == 0) return;

            if (rootFolder.IsNullOrEmpty()) return;

            string obfuscationOutputFolder = Path.Combine(rootFolder, "BabelOut");

            #region  캐쉬 삭제
            if (true == Directory.Exists(obfuscationOutputFolder))
            {
                DirectoryInfo folderInfo = new DirectoryInfo(obfuscationOutputFolder);
                var files = folderInfo.GetFiles();
                var directories = folderInfo.GetDirectories("*.*", SearchOption.AllDirectories);
                if (files != null && files.Count() != 0)
                {
                    foreach (var file in files)
                    {
                        File.Delete(file.FullName);
                    }
                }

                if (directories != null && directories.Count() != 0)
                {
                    foreach (var directory in directories)
                        directory.Delete(true);
                }
            }

            //DirectoryInfo signFolder = new DirectoryInfo(@"C:\CodeSign");
            //var signFiles = signFolder.GetFiles();
            //if (signFiles != null && signFiles.Count() != 0)
            //{
            //    foreach (var file in signFiles)
            //    {
            //        File.Delete(file.FullName);
            //    }
            //}

            #endregion

            foreach (var file in fileList.Where(w => true == w.IsNeedObfuscation))
            {
                try
                {
                    var psi = new ProcessStartInfo(BabelPath)
                            {
                                Arguments = string.Format(@"""{0}""", file.FileFullPath),
                                UseShellExecute = true,
                                CreateNoWindow = false
                            };
                    var proc = Process.Start(psi);
                    if (proc != null)
                    {
                        proc.WaitForExit();
                    }

                    file.IsObfuscated = true;
                }
                catch (Exception e)
                {
                    MessageBox.Show(string.Format("난독화 실패.\r\n{0}", e.Message));
                }
            }

            foreach (var file in fileList)
            {
                try
                {
                    if (true == file.IsNeedObfuscation)
                    {
                        string obfuscatedFile = Path.Combine(obfuscationOutputFolder, file.FileName);
                        if (true == File.Exists(obfuscatedFile))
                        {
                            File.Copy(obfuscatedFile, Path.Combine(CodeSignTargetPath, file.FileName));
                        }
                    }
                    else
                    {
                        if (true == File.Exists(file.FileFullPath))
                        {
                            File.Copy(file.FileFullPath, Path.Combine(CodeSignTargetPath, file.FileName));
                        }
                    }
                }
                catch (Exception e)
                {
                    MessageBox.Show(string.Format("파일 복사 실패\r\n{0}", e.Message));
                }
            }
        }

        private DelegateCommand codeSignCommand;

        public DelegateCommand CodeSignCommand
        {
            get
            {
                if (codeSignCommand == null)
                    codeSignCommand = new DelegateCommand(ExecuteCodeSign);
                return codeSignCommand;
            }
        }

        private void ExecuteCodeSign()
        {
            if (fileList == null || fileList.Count == 0) return;
            if (document == null || document.Root == null || false == document.Root.HasAttributes) return;

            var certAttribute = document.Root.Attributes("CertPath").FirstOrDefault();
            if (certAttribute == null || certAttribute.Value.IsNullOrEmpty())
            {
                MessageBox.Show("인증서 경로가 설정되지 않았습니다.");
                return;
            }

            var pwAttribute = document.Root.Attributes("CertPW").FirstOrDefault();
            if (pwAttribute == null || pwAttribute.Value.IsNullOrEmpty())
            {
                MessageBox.Show("인증서 암호가 설정되지 않았습니다.");
                return;
            }

            foreach (var file in fileList)
            {
                string signPath = Path.Combine(CodeSignTargetPath, file.FileName);

                try
                {
                    if (true == File.Exists(signPath))
                    {
                        var psi = new ProcessStartInfo(SignToolPath)
                            {
                                Arguments = string.Format(@"sign /v /f ""{1}"" /p {2} /fd sha256 /td sha256 /tr http://sha256timestamp.ws.symantec.com/sha256/timestamp ""{0}""",
                                signPath,
                                certAttribute.Value, pwAttribute.Value),
                                UseShellExecute = false,
                                CreateNoWindow = false,
                                RedirectStandardOutput = true
                            };
                        var proc = Process.Start(psi);
                        if (proc != null)
                        {
                            file.CodeSignMessage = proc.StandardOutput.ReadToEnd();
                            proc.WaitForExit();
                        }
                    }
                }
                catch (Exception e)
                {
                    MessageBox.Show(string.Format("CodeSign 오류\r\n{0}", e.Message));
                }
            }
        }

        private DelegateCommand clearCacheCommand;

        public DelegateCommand ClearCacheCommand
        {
            get
            {
                if (clearCacheCommand == null)
                    clearCacheCommand = new DelegateCommand(ExecuteClearCache);

                return clearCacheCommand;
            }
        }

        private void ExecuteClearCache()
        {
            if (rootFolder.IsNotNullOrEmpty())
            {
                string obfuscationOutputFolder = Path.Combine(rootFolder, "BabelOut");
                if (true == Directory.Exists(obfuscationOutputFolder))
                {
                    DirectoryInfo folderInfo = new DirectoryInfo(obfuscationOutputFolder);
                    var files = folderInfo.GetFiles();
                    var directories = folderInfo.GetDirectories("*.*", SearchOption.AllDirectories);
                    if (files != null && files.Count() != 0)
                    {
                        foreach (var file in files)
                        {
                            File.Delete(file.FullName);
                        }
                    }

                    if (directories != null && directories.Count() != 0)
                    {
                        foreach (var directory in directories)
                            directory.Delete(true);
                    }
                }
            }

            DirectoryInfo signFolder = new DirectoryInfo(@"C:\CodeSign");
            var signFiles = signFolder.GetFiles();
            if (signFiles != null && signFiles.Count() != 0)
            {
                foreach (var file in signFiles)
                {
                    File.Delete(file.FullName);
                }
            }
        }

        private DelegateCommand updateXmlTimeCommand;

        public DelegateCommand UpdateXmlTimeCommand
        {
            get
            {
                if (updateXmlTimeCommand == null)
                    updateXmlTimeCommand = new DelegateCommand(ExecuteUpdateXmlTime);
                return updateXmlTimeCommand;
            }
        }

        private void ExecuteUpdateXmlTime()
        {
            //var folderDialog = new System.Windows.Forms.FolderBrowserDialog();
            //folderDialog.SelectedPath = System.IO.Directory.GetCurrentDirectory();

            //if (System.Windows.Forms.DialogResult.OK != folderDialog.ShowDialog()) return;

            if (MessageBoxResult.OK != MessageBox.Show("XConfig, XHibernate 폴더를 기준으로 *.xml 시간을 변경하시겠습니까?", "", MessageBoxButton.OKCancel)) return;

            DateTime setupTime = DateTime.Now;

            List<FileInfo> hibernateMappingFiles = new List<FileInfo>();

            DirectoryInfo xconfig = new DirectoryInfo(XConfigPath);
            DirectoryInfo xhibernate = new DirectoryInfo(XHibernatePath);

            if (true == Directory.Exists(xconfig.FullName))
            {
                hibernateMappingFiles.AddRange(xconfig.GetFiles("*.xml", SearchOption.AllDirectories));
            }

            if (true == Directory.Exists(xhibernate.FullName))
            {
                hibernateMappingFiles.AddRange(xhibernate.GetFiles("*.xml", SearchOption.AllDirectories));
            }

            if (hibernateMappingFiles.Count() != 0)
            {
                foreach (var file in hibernateMappingFiles)
                {
                    file.CreationTime = setupTime;
                    file.LastWriteTime = setupTime;
                    file.LastAccessTime = setupTime;
                }

                MessageBox.Show("Xml Write & Creation time update completed.");
            }
        }

        private DelegateCommand assemblyVersionControlCommand;

        public DelegateCommand AssemblyVersionControlCommand
        {
            get
            {
                if (assemblyVersionControlCommand == null)
                    assemblyVersionControlCommand = new DelegateCommand(ExecuteAssemblyVersionControl);
                return assemblyVersionControlCommand;
            }
        }

        private void ExecuteAssemblyVersionControl()
        {
            var win = new AssemblyVersionControlWindow();
            win.Owner = this.WindowInstance;
            win.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            win.ShowDialog();
        }

        private DelegateCommand<ObfusFileModel> deleteFileCommand;

        public DelegateCommand<ObfusFileModel> DeleteFileCommand
        {
            get
            {
                if (deleteFileCommand == null)
                    deleteFileCommand = new DelegateCommand<ObfusFileModel>(ExecuteDeleteFileCommand);
                return deleteFileCommand;
            }
        }

        private void ExecuteDeleteFileCommand(ObfusFileModel param)
        {
            if (param == null) return;
            if (fileList == null || fileList.Count == 0) return;

            var item = fileList.FirstOrDefault(f => f.FileFullPath == param.FileFullPath);
            if (item != null) fileList.Remove(item);
        }

        public void OnFileDrop(string[] filepaths)
        {
            if (ruleList == null || ruleList.Count == 0) return;

            if (fileList == null)
                fileList = new ObservableCollection<ObfusFileModel>();

            if (filepaths != null && filepaths.Count() != 0)
            {
                var parent = Directory.GetParent(filepaths[0]);
                RootFolder = parent.FullName;

                List<ObfusFileModel> list = new List<ObfusFileModel>();

                foreach (var file in filepaths)
                {
                    var model = new ObfusFileModel { FileFullPath = file, FileName = Path.GetFileName(file), Extension = Path.GetExtension(file) };

                    if (true == ruleList.Any(a => a.ModuleName == model.FileName))
                    {
                        model.IsNeedObfuscation = true;
                    }
                    else
                    {
                        model.IsNeedObfuscation = false;
                    }

                    if (model.Extension == ".exe" || model.Extension == ".dll")
                    {
                        list.Add(model);
                    }
                }

                if (list.Count != 0)
                {
                    foreach (var item in list.OrderBy(o => o.FileName))
                    {
                        if (0 == fileList.Count(c => c.FileFullPath == item.FileFullPath))
                        {
                            FileList.Add(item);
                        }
                    }
                }
            }
        }
    }
}
