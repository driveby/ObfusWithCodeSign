using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace ObfusWithSignTool
{
    /// <summary>
    /// Interaction logic for ObfuscationRuleSettingWindow.xaml
    /// </summary>
    public partial class ObfuscationSettingWindow : Window
    {
        private WeakReference weakReference;

        public ObfuscationSettingWindowViewModel Context
        {
            get
            {
                if (weakReference == null || weakReference.IsAlive == false)
                    return null;

                return (ObfuscationSettingWindowViewModel)weakReference.Target;
            }
        }

        public ObfuscationSettingWindow()
        {
            InitializeComponent();

            var dataContext = new ObfuscationSettingWindowViewModel();
            dataContext.WindowInstance = this;

            weakReference = new WeakReference(dataContext);
            this.DataContext = this.Context;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (Context != null) Context.Loaded();
        }

        private void Window_Unloaded(object sender, RoutedEventArgs e)
        {
            if (Context != null) Context.Unloaded();
        }
    }
}
