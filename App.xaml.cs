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

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            ReadModInfo();

            // Check if manager.ini exists and if ModDirectory is set

            string currentDirectory = Environment.CurrentDirectory;
            string disabled = configManager.DisabledMods;
            bool disabledExists = Directory.Exists(disabled);

            if (string.IsNullOrEmpty(configManager.DisabledMods))
            {
                configManager.DisabledMods = currentDirectory + @"\_disabled";
                configManager.SaveConfiguration();
            }

            if (!disabledExists)
            {
                Directory.CreateDirectory(PathIO.Combine(configManager.DisabledMods, @"\_disabled"));
            }

            if (string.IsNullOrEmpty(configManager.ModDirectory))
            {
                // Show a message box asking the user for the mod directory
                MessageBoxResult result = MessageBox.Show("Mod directory not found in the INI file. Do you want to set it now?", "Set Mod Directory", MessageBoxButton.YesNo, MessageBoxImage.Question);
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

        }

        private void ReadModInfo()
        {
            string modDir = configManager.ModDirectory;

            string modInfoPath = PathIO.Combine(modDir, "mod_info.txt");

            using (StreamWriter writer = new StreamWriter(modInfoPath, false, Encoding.UTF8))
            {
                string[] manifestFiles = Directory.GetFiles(modDir, "manifest.json", SearchOption.AllDirectories);

                foreach (string manifestPath in manifestFiles) 
                {
                    try
                    {
                        string manifestData = File.ReadAllText(manifestPath, Encoding.UTF8);
                        dynamic manifestJson = Newtonsoft.Json.JsonConvert.DeserializeObject(manifestData);
                        string modName = manifestJson.Name ?? "Unknown Mod";
                        string modVersion = manifestJson.ModVersion ?? "Unknown Version";
                        string gameVersion = manifestJson.GameVersion ?? "Unknown Game Version";

                        writer.WriteLine($"(Mod Name: {modName}, Version: {modVersion}, Game Version: {gameVersion})");
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine($"Error reading: {manifestPath}");
                        Console.WriteLine($"Except message: {e.Message}");
                        Console.WriteLine($"Skipping this manifest file.");
                    }
                }
            }
        }
    }
}