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
using System.IO;
using System.Security.Cryptography.X509Certificates;
using PathIO = System.IO.Path;
using System.Data.SQLite;
using System.Runtime.CompilerServices;

namespace Blade_Sorcery_ModManager.View.UserControls
{

    //MAKE THE FUCKING ENABLING DISABLING WORK NIGGGGGGGGAAAAAAAA!!!!!!!!!!!!
    //is almost working :D

    public partial class ModSections : UserControl
    {
        ConfigManager config = new ConfigManager();

        public static readonly DependencyProperty ModNumberProperty =
            DependencyProperty.Register("ModNumber", typeof(int), typeof(ModSections), new PropertyMetadata(1));

        public string ModNameBtn { get; private set; }

        public int ModNumberProp
        {
            get { return (int)GetValue(ModNumberProperty); }
            set { SetValue(ModNumberProperty, value); }
        }

        public static readonly DependencyProperty ModNameProperty =
            DependencyProperty.Register("ModName", typeof(string), typeof(ModSections), new PropertyMetadata("ModName"));

        public string ModNameProp
        {
            get { return (string)GetValue(ModNameProperty); }
            set { SetValue(ModNameProperty, value); }
        }  

        public ModSections()
        {
            InitializeComponent();
            EnableDisableButton.Tag = "Enabled";
        }

        static string connectString = "Data Source=mod_database.db;Version=3;";

        public void LoadModData(string modName, string modVersion, string gameVersion)
        {
            try
            {
                ModName.Text = modName;
            }
            catch (Exception ex) 
            {
                MessageBox.Show($"Error loading mod data: {ex.Message}");
            }
        }

        private void EnableDisableButton_Click(object sender, RoutedEventArgs e)
        {

            try
            {
                Button button = (Button)sender;

                Brush enabledColour = (Brush)FindResource("EnabledBtnBgBrush");

                string modName = ModName.Text; // Get the mod name from the TextBlock
                string modFolderPath = PathIO.Combine(config.ModDirectory, modName); // Mod folder path
                string disabledFolderPath = PathIO.Combine(config.DisabledMods, modName); // Disabled mod folder path

                if ((string)button.Tag == "Enabled")
                {
                    using (DataManager _manager = new DataManager(connectString))
                    {
                        _manager.UpdateStateData(modName, "Disabled");
                    }

                    button.Content = "Disabled";
                    button.Background = Brushes.Red;
                    button.Tag = "Disabled";

                    // Create the disabled mod directory if it doesn't already exist
                    Directory.CreateDirectory(PathIO.Combine(config.DisabledMods));

                    // Copy the contents of the mod folder to the disabled folder
                    CopyDirectory(modFolderPath, disabledFolderPath);

                    // Delete the mod folder
                    Directory.Delete(modFolderPath, true);

                }
                else if ((string)button.Tag == "Disabled")
                {
                    // Enable the mod

                    using (DataManager _manager = new DataManager(connectString))
                    {
                        _manager.UpdateStateData(modName, "Enabled");
                    }

                    button.Content = "Enabled";
                    button.Background = enabledColour;
                    button.Tag = "Enabled";

                    if (!Directory.Exists(disabledFolderPath))
                    {
                        // Handle the case where the mod is not found in the _disabled directory
                        MessageBox.Show($"Mod '{modName}' not found in the _disabled directory.");
                        return;
                    }

                    // Copy the contents of the disabled folder back to the mod folder
                    CopyDirectory(disabledFolderPath, modFolderPath);

                    // Delete the disabled folder
                    Directory.Delete(disabledFolderPath, true);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in EnableDisableButton_Click: {ex.Message}");
            }
        }

        private void CopyDirectory(string sourceDir, string destDir)
        {

            //Ideally copies an entire select folder to another path. "sourceDir" is the folder you want to copy, "destDir" is the place you want your copied folder to go

            Directory.CreateDirectory(destDir);

            foreach (string file in Directory.GetFiles(sourceDir))
            {
                string destFile = PathIO.Combine(destDir, PathIO.GetFileName(file));
                File.Copy(file, destFile, true);
            }

            foreach (string subDir in Directory.GetDirectories(sourceDir))
            {
                string destSubDir = PathIO.Combine(destDir, PathIO.GetFileName(subDir));
                CopyDirectory(subDir, destSubDir);
            }
        }
    }
}