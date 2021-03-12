using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.Windows;

namespace ObfusWithSignTool
{
    public class ObfuscationSettingWindowViewModel : BindableBase
    {
        private XDocument document;

        private readonly string RuleFilePath = System.IO.Directory.GetCurrentDirectory() + "\\Rule.xml";

        public void Loaded()
        {
            ruleList = new ObservableCollection<RuleModel>();

            if (false == File.Exists(RuleFilePath))
            {
                XElement xmlRoot = new XElement("Rules");
                XAttribute certificatePath = new XAttribute("CertPath", "");
                XAttribute certPassword = new XAttribute("CertPW", "");
                xmlRoot.Add(certificatePath, certPassword);
                document = new XDocument(xmlRoot);
                document.Save(RuleFilePath);
            }
            else
            {
                document = XDocument.Load(RuleFilePath);
            }

            if (document.Root != null)
            {
                if (true == document.Root.HasAttributes)
                {
                    var pathAttribute = document.Root.Attributes("CertPath").FirstOrDefault();
                    if (pathAttribute != null) this.CertificateFilePath = pathAttribute.Value;

                    var pwAttribute = document.Root.Attributes("CertPW").FirstOrDefault();
                    if (pwAttribute != null) this.CertificatePassword = pwAttribute.Value;
                }

                if (true == document.Root.HasElements)
                {
                    List<RuleModel> list = new List<RuleModel>();

                    foreach (var element in document.Root.Elements("Rule"))
                    {
                        list.Add(new RuleModel { ModuleName = element.Value });
                    }

                    if (list.Count != 0)
                    {
                        RuleList = new ObservableCollection<RuleModel>(list.OrderBy(o => o.ModuleName));
                    }
                }
            }
        }

        public void Unloaded()
        {
            this.document = null;
        }

        public ObfuscationSettingWindow WindowInstance { get; set; }

        private string certificateFilePath;

        public string CertificateFilePath
        {
            get { return certificateFilePath; }
            set { certificateFilePath = value; OnPropertyChanged("CertificateFilePath"); }
        }

        private string certificatePassword;

        public string CertificatePassword
        {
            get { return certificatePassword; }
            set { certificatePassword = value; OnPropertyChanged("CertificatePassword"); }
        }

        private ObservableCollection<RuleModel> ruleList;

        public ObservableCollection<RuleModel> RuleList
        {
            get { return ruleList; }
            set { ruleList = value; OnPropertyChanged("RuleList"); }
        }

        private DelegateCommand openCertificateFileCommand;

        public DelegateCommand OpenCertificateFileCommand
        {
            get 
            {
                if (openCertificateFileCommand == null)
                    openCertificateFileCommand = new DelegateCommand(ExecuteOpenCertificateFile);
                return openCertificateFileCommand; 
            }
        }

        private void ExecuteOpenCertificateFile()
        {
            var openDialog = new OpenFileDialog();
            openDialog.Filter = "Certificate Files (*.pfx)| *.pfx";

            if (false == openDialog.ShowDialog())
                return;

            if (false == File.Exists(openDialog.FileName))
                return;

            this.CertificateFilePath = openDialog.FileName;
        }

        private DelegateCommand addRuleCommand;

        public DelegateCommand AddRuleCommand
        {
            get
            {
                if (addRuleCommand == null)
                    addRuleCommand = new DelegateCommand(ExecuteAddRule);
                return addRuleCommand;
            }
        }

        private void ExecuteAddRule()
        {
            var openDialog = new OpenFileDialog();
            openDialog.Filter = "Assembly Files (*.exe; *.dll)| *.exe; *.dll";

            if (false == openDialog.ShowDialog())
                return;

            if (false == File.Exists(openDialog.FileName))
                return;

            if (ruleList.Count == 0)
            {
                RuleList.Add(new RuleModel { ModuleName = Path.GetFileName(openDialog.FileName) });
            }
            else
            {
                if (false == ruleList.Any(a => a.ModuleName == Path.GetFileName(openDialog.FileName)))
                {
                    RuleList.Add(new RuleModel { ModuleName = Path.GetFileName(openDialog.FileName) });
                }
            }

            if (document != null && document.Root != null)
            {
                document.Root.RemoveNodes();

                foreach (var rule in ruleList)
                {
                    var element = new XElement("Rule");
                    element.Value = rule.ModuleName;
                    document.Root.Add(element);
                }

                document.Save(RuleFilePath);
            }
        }

        private DelegateCommand<RuleModel> removeRuleCommand;

        public DelegateCommand<RuleModel> RemoveRuleCommand
        {
            get
            {
                if (removeRuleCommand == null)
                    removeRuleCommand = new DelegateCommand<RuleModel>(ExecuteRemoveRule);
                return removeRuleCommand;
            }
        }

        private void ExecuteRemoveRule(RuleModel param)
        {
            if (param == null) return;
            if (ruleList == null || ruleList.Count == 0) return;

            var binding = ruleList.FirstOrDefault(f => f.ModuleName == param.ModuleName);
            ruleList.Remove(binding);

            XElement removeNode = null;

            if (document != null && document.Root != null && document.Root.HasElements)
            {
                foreach (var element in document.Root.Elements("Rule"))
                {
                    if (element.Value == param.ModuleName)
                    {
                        removeNode = element;
                        break;
                    }
                }

                if (removeNode != null)
                {
                    removeNode.Remove();

                    document.Save(RuleFilePath);
                }
            }
        }

        private DelegateCommand saveCommand;

        public DelegateCommand SaveCommand
        {
            get
            {
                if (saveCommand == null)
                    saveCommand = new DelegateCommand(ExecuteSave);
                return saveCommand; 
            }
        }

        private void ExecuteSave()
        {
            if (document == null || document.Root == null) return;

            if(certificateFilePath.IsNullOrEmpty())
            {
                MessageBox.Show("인증서 경로가 설정되지 않았습니다.");
                return;
            }

            if (false == document.Root.HasAttributes)
            {
                XAttribute certificatePath = new XAttribute("CertPath", this.certificateFilePath);
                XAttribute certPassword = new XAttribute("CertPW", this.certificatePassword.IsNullOrEmpty() ? "" : certificatePassword);

                document.Root.Add(certificatePath, certPassword);
            }
            else
            {
                var pathAttribute = document.Root.Attributes("CertPath").FirstOrDefault();
                if (pathAttribute != null) pathAttribute.Value = certificateFilePath;

                var pwAttribute = document.Root.Attributes("CertPW").FirstOrDefault();
                if (pwAttribute != null) pwAttribute.Value = certificatePassword;
            }

            document.Save(RuleFilePath);
        }
    }
}
