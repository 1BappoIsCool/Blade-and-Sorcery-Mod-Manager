using Blade_Sorcery_ModManager.View.UserControls;
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
using Microsoft.WindowsAPICodePack.Dialogs;
using System.IO;
using PathIO = System.IO.Path;
using System.Diagnostics;
using Microsoft.Win32;
using System.Data.SQLite;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using System.IO.Compression;

namespace Blade_Sorcery_ModManager
{
    public partial class MainWindow : System.Windows.Window
    {
        ConfigManager config = new ConfigManager();

        private ModSections modSections;

        public MainWindow()
        {
            modSections = new ModSections();
            InitializeComponent();
            InitializeModList();
        }

        static string connectString = "Data Source=mod_database.db;Version=3;";
        DataManager _manager = new DataManager(connectString);

        private void InitializeModList()
        {
            List<Tuple<string, string, string>> modData = _manager.ReadData();

            for (int lineNumber = 0; lineNumber < modData.Count; lineNumber++)
            {
                // Add a new RowDefinition for each user control
                ModList.RowDefinitions.Add(new RowDefinition { Height = new GridLength(80) });

                ModSections modSection = new ModSections();
                modSection.HorizontalAlignment = HorizontalAlignment.Left;
                modSection.Margin = new Thickness(10, 0, 0, 0);
                modSection.Padding = new Thickness(1);
                modSection.SetValue(Grid.RowProperty, ModList.RowDefinitions.Count - 1);

                Tuple<string, string, string> modTuple = modData[lineNumber];
                modSection.LoadModData(modTuple.Item1, modTuple.Item2, modTuple.Item3);

                // Set the ModNumber directly with the loop index (line number)
                modSection.ModNumber.Text = (lineNumber + 1).ToString();

                ModList.Children.Add(modSection);
            }
        }

        private void btnOpenModFolder_Click(object sender, RoutedEventArgs e)
        {
            string modsFolder = config.ModDirectory;

            if (Directory.Exists(modsFolder))
            {
                Process.Start("explorer.exe", modsFolder);
            }
            else
            {
                MessageBoxResult result = MessageBox.Show("Mods folder was not found, do you want to set it now?", "Set Mod Directory", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                {
                    // Show a folder selection dialog to select the mod directory
                    var folderDialog = new CommonOpenFileDialog();
                    folderDialog.IsFolderPicker = true;

                    if (folderDialog.ShowDialog() == CommonFileDialogResult.Ok)
                    {
                        string selectedFolderPath = PathIO.GetDirectoryName(folderDialog.FileName);

                        string modDirectoryPath = PathIO.Combine(selectedFolderPath, "Mods");

                        // Save the selected directory to the INI file
                        config.ModDirectory = modDirectoryPath;
                        config.SaveConfiguration();
                    }
                    else
                    {
                        // Handle the case where the user cancels the folder selection dialog
                        MessageBox.Show("Mod directory not set. The application may not work correctly without a mod directory.");
                    }

                }
            }
        }

        public ModSections FindModSectionByName(string modName)
        {
            foreach (var child in ModList.Children)
            {
                if (child is ModSections modSection && modSection.ModName.Text == modName)
                {
                    return modSection;
                }
            }
            return null;
        }

        private async void Window_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                foreach (string file in files)
                {
                    string extension = PathIO.GetExtension(file).ToLower();

                    if (extension == ".zip" || extension == ".rar")
                    {
                        await InstallAsync(file);
                    }
                }
            }
        }

        private async Task InstallAsync(string filePath)
        {
            // Update UI as installation starts
            // Show progress bar, set progress to 0

            // Get the destination path from the textbox
            string destinationPath = config.ModDirectory;

            // Extract the entire archive to the destination directory
            ZipFile.ExtractToDirectory(filePath, destinationPath);
        }
    }
}
