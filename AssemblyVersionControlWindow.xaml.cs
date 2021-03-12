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
    /// Interaction logic for AssemblyVersionControlWindow.xaml
    /// </summary>
    public partial class AssemblyVersionControlWindow : Window
    {
        private WeakReference weakReference;

        public AssemblyVersionControlWindowViewModel Context
        {
            get
            {
                if (weakReference == null || weakReference.IsAlive == false)
                    return null;

                return (AssemblyVersionControlWindowViewModel)weakReference.Target;
            }
        }

        public AssemblyVersionControlWindow()
        {
            InitializeComponent();

            var dataContext = new AssemblyVersionControlWindowViewModel();
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

        private void Grid_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            var grid = sender as Grid;

            if (grid.DataContext == null) return;

            if (Context == null) return;

            AssemblyInfoModel assembly = grid.DataContext as AssemblyInfoModel;

            if (assembly == null) return;

            if (false == assembly.IsExcepted)
            {
                MenuItem menu = new MenuItem();
                menu.Header = "제외";
                menu.Command = Context.ExceptItemCommand;
                menu.CommandParameter = assembly;
                grid.ContextMenu = new ContextMenu();
                grid.ContextMenu.Items.Add(menu);
            }
            else
            {
                MenuItem menu = new MenuItem();
                menu.Header = "포함";
                menu.Command = Context.ExceptItemCommand;
                menu.CommandParameter = assembly;
                grid.ContextMenu = new ContextMenu();
                grid.ContextMenu.Items.Add(menu);
            }
        }

        private void Image_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (Context == null) return;

            Context.RefreshCommand.Execute();
        }
    }
}
