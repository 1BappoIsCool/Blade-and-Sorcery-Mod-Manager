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
using Blade_Sorcery_ModManager.View.UserControls;

namespace Blade_Sorcery_ModManager
{
    internal class ModReader
    {
        ConfigManager configManager = new ConfigManager();

        public void ReadModInfo()
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
