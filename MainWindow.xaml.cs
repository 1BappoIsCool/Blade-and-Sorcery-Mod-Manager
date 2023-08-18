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

namespace Blade_Sorcery_ModManager
{
    public partial class MainWindow : Window
    {
        ConfigManager config = new ConfigManager();

        private ModSections modSections;

        public MainWindow()
        {
            modSections = new ModSections();

            InitializeComponent();
            InitializeModList();
            modSections.InitModStates();
        }

        private ModSections FindModSectionByName(string modName)
        {
            return ModList.Children.OfType<ModSections>().FirstOrDefault(modSection =>
                modSection.ModName.Text.Equals(modName, StringComparison.OrdinalIgnoreCase));
        }

        private void UpdDisabledBtns()
        {
            try
            {
                string[] disabledFolders = Directory.GetDirectories(config.ModDirectory, "*_DisabledPlaceholder*");

                foreach (string disabledFolder in disabledFolders)
                {
                    string manifestPath = PathIO.Combine(disabledFolder, "manifest.json");

                    if (File.Exists(manifestPath))
                    {
                        string manifestData = File.ReadAllText(manifestPath, Encoding.UTF8);
                        dynamic manifestJson = Newtonsoft.Json.JsonConvert.DeserializeObject(manifestData);
                        string modName = manifestJson.Name ?? "Unknown Mod";

                        // Find the ModSections control with the corresponding ModName
                        ModSections modSection = FindModSectionByName(modName);

                        if (modSection != null)
                        {
                            // Update the button content and background color for disabled mods
                            modSection.EnableDisableButton.Content = "Disabled";
                            modSection.EnableDisableButton.Background = Brushes.Red;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error updating buttons for disabled mods: {ex.Message}");
            }
        }

        private void InitializeModList()
        {
            UpdDisabledBtns();

            try
            {
                string modInfoFilePath = PathIO.Combine(config.ModDirectory, "mod_info.txt");

                if (File.Exists(modInfoFilePath))
                {
                    string[] lines = File.ReadAllLines(modInfoFilePath);
                    for (int lineNumber = 0; lineNumber < lines.Length; lineNumber++)
                    {
                        // Add a new RowDefinition for each user control
                        ModList.RowDefinitions.Add(new RowDefinition { Height = new GridLength(80) });

                        ModSections modSection = new ModSections();
                        modSection.HorizontalAlignment = HorizontalAlignment.Left;
                        modSection.Margin = new Thickness(10, 0, 0, 0);
                        modSection.Padding = new Thickness(1);
                        modSection.SetValue(Grid.RowProperty, ModList.RowDefinitions.Count - 1);

                        modSection.LoadModData(modInfoFilePath, lineNumber);

                        ModList.Children.Add(modSection);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}");
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            string disabledMods = config.DisabledMods;
            MessageBox.Show($"Disabled Mods Path: " + disabledMods);
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
    }
}
