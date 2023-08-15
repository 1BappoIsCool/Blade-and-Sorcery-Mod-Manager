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

namespace Blade_Sorcery_ModManager
{
    /// <summary>
    /// Interaction logic for ModDirectoryInputDialog.xaml
    /// </summary>
    public partial class ModDirectoryInputDialog : Window
    {
        public string UserInput { get; private set; }

        public ModDirectoryInputDialog()
        {
            InitializeComponent();
        }

        private void OK_Click(object sender, RoutedEventArgs e)
        {
            UserInput = ModDirTextBox.Text;
            DialogResult = true;
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
