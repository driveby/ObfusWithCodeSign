using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ObfusWithSignTool
{
    public class RuleModel : BindableBase
    {
        string moduleName;

        public string ModuleName
        {
            get { return moduleName; }
            set { moduleName = value; OnPropertyChanged("ModuleName"); }
        }
    }
}
