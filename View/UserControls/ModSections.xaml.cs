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

namespace Blade_Sorcery_ModManager.View.UserControls
{

    //MAKE THE FUCKING ENABLING DISABLING WORK NIGGGGGGGGAAAAAAAA!!!!!!!!!!!!
    //is almost working :D

    public partial class ModSections : UserControl
    {
        ModReader reader = new ModReader();


        ConfigManager config = new ConfigManager();

        private string statesFilePath = PathIO.Combine(Environment.CurrentDirectory, "mod_states.txt");

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

        public void InitModStates()
        {
            string[] modInfoLines = File.ReadAllLines(PathIO.Combine(config.ModDirectory, "mod_info.txt"));
            List<string> modStates = new List<string>();

            foreach (string line in modInfoLines)
            {
                if (!string.IsNullOrWhiteSpace(line) && line.Contains("(Mod Name:"))
                {
                    int startIndex = line.IndexOf("(Mod Name:") + "(Mod Name:".Length;
                    int endIndex = line.IndexOf(",", startIndex);
                    string modName = line.Substring(startIndex, endIndex - startIndex).Trim();

                    // Read the mod's state from mod_states.txt
                    string modState = "Enabled";
                    string[] statesLines = File.ReadAllLines(statesFilePath);
                    foreach (string stateLine in statesLines)
                    {
                        string[] parts = stateLine.Split(',');
                        if (parts.Length == 2 && parts[0].Trim() == modName)
                        {
                            modState = parts[1].Trim();
                            break;
                        }
                    }

                    modStates.Add($"{modName}, {modState}");
                }
            }

            // Write the modified content back to mod_states.txt
            File.WriteAllLines(statesFilePath, modStates);
        }

        public void UpdateModState(string modName, bool enabled)
        {
            // Read the content of the mod_states.txt file
            List<string> modStates = File.ReadAllLines(statesFilePath).ToList();

            // Find the mod entry and update its state
            for (int i = 0; i < modStates.Count; i++)
            {
                string[] parts = modStates[i].Split(',');
                if (parts.Length == 2 && parts[0].Trim() == modName)
                {
                    modStates[i] = $"{modName}, {(enabled ? "Enabled" : "Disabled")}";
                    break;
                }
            }

            // Write the modified content back to the mod_states.txt file
            File.WriteAllLines(statesFilePath, modStates);
        }

        public void LoadModData(string filePath, int lineNumber)
        {
            try
            {
                string[] lines = File.ReadAllLines(filePath);
                if (lineNumber >= 0 && lineNumber < lines.Length)
                {
                    string line = lines[lineNumber];
                    if (!string.IsNullOrWhiteSpace(line) && line.Contains("(Mod Name:"))
                    {
                        int startIndex = line.IndexOf("(Mod Name:") + "(Mod Name:".Length;
                        int endIndex = line.IndexOf(",", startIndex);
                        ModName.Text = line.Substring(startIndex, endIndex - startIndex).Trim();
                        ModNumber.Text = Convert.ToString(lineNumber + 1);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading mod data: {ex.Message}");
            }
        }

        private void EnableDisableButton_Click(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;

            string modName = ModName.Text; // Get the mod name from the TextBlock
            string modFolderPath = PathIO.Combine(config.ModDirectory, modName); // Mod folder path
            string disabledFolderPath = PathIO.Combine(config.DisabledMods, modName); // Disabled mod folder path

            if ((string)button.Tag == "Enabled")
            {
                // Create the disabled mod directory if it doesn't already exist
                Directory.CreateDirectory(PathIO.Combine(config.DisabledMods));

                // Copy the contents of the mod folder to the disabled folder
                CopyDirectory(modFolderPath, disabledFolderPath);

                // Delete the mod folder
                Directory.Delete(modFolderPath, true);

                UpdateModState(modName, false);

                button.Content = "Disabled";
                button.Background = Brushes.Red;
                button.Tag = "Disabled"; // Update the tag
            }
            else if ((string)button.Tag == "Disabled")
            {
                // Enable the mod

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

                UpdateModState(modName, true);

                button.Content = "Enabled";
                button.Background = Brushes.Green;
                button.Tag = "Enabled"; // Update the tag
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