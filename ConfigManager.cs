using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using IniParser;
using IniParser.Model;
using System.Windows;

namespace Blade_Sorcery_ModManager
{
    internal class ConfigManager
    {
        private const string IniFilePath = "manager.ini";

        public string ModDirectory {get; set;}
        public string DisabledMods { get; set; }

        public ConfigManager()
        {
            LoadConfiguration();
        }

        private void LoadConfiguration()
        {
            var parser = new FileIniDataParser();
            if (File.Exists(IniFilePath))
            {
                IniData data = parser.ReadFile(IniFilePath);
                ModDirectory = data["Paths"]["ModDirectory"];
                DisabledMods = data["Paths"]["DisabledMods"];
            }
            else
            {
                ModDirectory = ""; // Set a default value or leave it empty
            }
        }

        public void SaveConfiguration()
        {
            var parser = new FileIniDataParser();
            IniData data = new IniData();
            data["Paths"]["ModDirectory"] = ModDirectory;
            data["Paths"]["DisabledMods"] = DisabledMods;

            parser.WriteFile(IniFilePath, data);
        }
    }
}
