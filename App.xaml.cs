using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.IO;
using PathIO = System.IO.Path;
using Microsoft.Win32;
using System.Net;
using Microsoft.WindowsAPICodePack.Dialogs;
using System.Windows.Shapes;
using System.Text;
using Newtonsoft.Json;
using Blade_Sorcery_ModManager.View.UserControls;
using System.Drawing;
using static Blade_Sorcery_ModManager.DataManager;
using System.Windows.Markup;

namespace Blade_Sorcery_ModManager
{
    
#pragma warning disable VSSpell001 // Spell Check
    public partial class App : Application
#pragma warning restore VSSpell001 // Spell Check
    {
        ConfigManager configManager = new ConfigManager();
        
        private MainWindow _mainWindow = new MainWindow();
        

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            string connectString = "Data Source=mod_database.db;Version=3;";
    
            configManager.DisabledMods = configManager.ModDirectory + @"\_disabled";
            configManager.SaveConfiguration();

            // Check if the appropriate paths are set

            if (!string.IsNullOrEmpty(configManager.ModDirectory)) // Check if DisabledMods is set
            {
                if (!Directory.Exists(configManager.ModDirectory)) // Check if the directory does not exist
                {
                    Directory.CreateDirectory(configManager.ModDirectory); // Create the _disabled directory
                }
            }

            if (string.IsNullOrEmpty(configManager.ModDirectory))
            {
                // Show a message box asking the user for the mod directory
                MessageBoxResult result = MessageBox.Show("A Mods folder was not found, would you like to set one now?", "Set Mod Directory", MessageBoxButton.YesNo, MessageBoxImage.Question);
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
                        configManager.ModDirectory = modDirectoryPath;
                        configManager.SaveConfiguration();
                    }
                    else
                    {
                        // Handle the case where the user cancels the folder selection dialog
                        MessageBox.Show("Mod directory not set. The application may not work correctly without a mod directory.");
                    }
                }
            }

            List<(string ModName, string State)> modStates;
            using (DataManager _manager = new DataManager(connectString))
            {
                try
                {
                    _manager.CreateTables();
                    modStates = _manager.GetAllStates();
                }
                catch (Exception ex)
                {
                    // Handle any exceptions that might occur during database operations
                    MessageBox.Show($"Error while retrieving mod states: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
            }

            foreach (var modState in modStates)
            {
                string modName = modState.ModName;
                string state = modState.State;

                try
                {
                    ModSections modSection = _mainWindow.FindModSectionByName(modName);
                    if (modSection != null)
                    {
                        if (state == ModState.Enabled.ToString())
                        {
                            modSection.EnableDisableButton.Content = "Enabled";
                            modSection.EnableDisableButton.Background = System.Windows.Media.Brushes.Green;
                            modSection.EnableDisableButton.Tag = "Enabled";
                        }
                        else if (state == ModState.Disabled.ToString())
                        {
                            modSection.EnableDisableButton.Content = "Disabled";
                            modSection.EnableDisableButton.Background = System.Windows.Media.Brushes.Red;
                            modSection.EnableDisableButton.Tag = "Disabled";
                        }
                    }
                }
                catch (Exception ex)
                {
                    // Handle any exceptions that might occur during UI interaction
                    MessageBox.Show($"Error while updating UI for mod {modName}: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    // You can choose to continue processing other mod states or return here, based on your requirement.
                }
            }

            List<Tuple<string, string, string>> modData;
            using (DataManager _manager = new DataManager(connectString))
            {
                modData = _manager.GatherData();
            }

            using (DataManager _manager = new DataManager(connectString))
            {
                foreach(var data in modData)
                {
                    string modName = data.Item1;
                    string modVersion = data.Item2;
                    string gameVersion = data.Item3;

                    _manager.InsertData(modName, modVersion, gameVersion);
                }
            }
        }
    }
}