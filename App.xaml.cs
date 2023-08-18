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

namespace Blade_Sorcery_ModManager
{
    
#pragma warning disable VSSpell001 // Spell Check
    public partial class App : Application
#pragma warning restore VSSpell001 // Spell Check
    {

        ConfigManager configManager = new ConfigManager();
        ModReader reader = new ModReader();

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

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

            reader.ReadModInfo();

        }
    }
}