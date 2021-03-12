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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ObfusWithSignTool
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private WeakReference weakReference;

        public MainWindowViewModel Context
        {
            get
            {
                if (weakReference == null || weakReference.IsAlive == false)
                    return null;

                return (MainWindowViewModel)weakReference.Target;
            }
        }

        public MainWindow()
        {
            InitializeComponent();

            var dataContext = new MainWindowViewModel();
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

        private void imgSetting_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (Context != null) Context.SettingCommand.Execute();
        }
    }
}
