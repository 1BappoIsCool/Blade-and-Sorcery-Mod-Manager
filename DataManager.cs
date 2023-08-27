using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Data.SQLite;
using System.Linq.Expressions;

namespace Blade_Sorcery_ModManager
{
    public class DataManager : IDisposable
    {

        private readonly SQLiteConnection _connection;
        string connectString = "Data Source=mod_database.db;Version=3;";

        private ConfigManager config = new ConfigManager();

        public enum ModState
        {
            Enabled,
            Disabled
        }

        public DataManager(string connectionString)
        {
            _connection = new SQLiteConnection(connectionString);
            _connection.Open();
        }

        public void Dispose()
        {
            _connection.Close();
            _connection.Dispose();
        }

        public void CreateTables()
        {
            string modTable = "CREATE TABLE IF NOT EXISTS Mods (ModName TEXT, ModVersion TEXT, GameVersion TEXT, State TEXT DEFAULT 'Enabled')";

            using (SQLiteCommand command = new SQLiteCommand(modTable, _connection))
            {
                command.ExecuteNonQuery();
            }
        }

        public List<Tuple<string, string, string>> GatherData()
        {
            List<Tuple<string, string, string>> modData = new List<Tuple<string, string, string>>();

            string modDir = config.ModDirectory;
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

                    modData.Add(Tuple.Create(modName, modVersion, gameVersion));
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Error reading: {manifestPath}");
                    Console.WriteLine($"Exception message: {e.Message}");
                    Console.WriteLine($"Skipping this manifest file.");
                }
            }

            return modData;
        }

        public void InsertData(string modName, string modVersion, string gameVersion)
        {
            CreateTables();

            string insQuery = @"
                   INSERT INTO Mods (ModName, ModVersion, GameVersion)
                   SELECT @ModName, @ModVersion, @GameVersion
                   WHERE NOT EXISTS (
                   SELECT 1 FROM Mods WHERE ModName = @ModName AND ModVersion = @ModVersion AND GameVersion = @GameVersion
                    );
                    ";

            using (SQLiteCommand command = new SQLiteCommand(insQuery, _connection))
            {
                command.Parameters.AddWithValue("@ModName", modName);
                command.Parameters.AddWithValue("@ModVersion", modVersion);
                command.Parameters.AddWithValue("@GameVersion", gameVersion);
                command.ExecuteNonQuery();
            }
        }

        public List<Tuple<string, string, string>> ReadData()
        {
            List<Tuple<string, string, string>> modData = new List<Tuple<string, string, string>>();

            using (SQLiteConnection connection = new SQLiteConnection(connectString))
            {
                connection.Open();

                string query = "SELECT ModName, ModVersion, GameVersion FROM Mods";
                using (SQLiteCommand command = new SQLiteCommand(query, connection))
                using (SQLiteDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        string modName = reader.GetString(0);
                        string modVersion = reader.GetString(1);
                        string gameVersion = reader.GetString(2);
                        modData.Add(new Tuple<string, string, string>(modName, modVersion, gameVersion));
                    }
                }

                connection.Close();
            }

            return modData;
        }

        public void UpdateStateData(string modName, string state)
        {
            try
            {
                string updQuery = @"
                    UPDATE Mods
                    SET State = @State
                    WHERE ModName = @ModName";

                using (SQLiteCommand command = new SQLiteCommand(updQuery, _connection))
                {
                    command.Parameters.AddWithValue("@State", state);
                    command.Parameters.AddWithValue("@ModName", modName);

                    command.ExecuteNonQuery();
                }
                
            }
            catch (Exception ex) 
            {
                Console.WriteLine($"Error updating mod state: {ex.Message}");
            }
        }

        public List<(string ModName, string State)> GetAllStates()
        {
            List<(string ModName, string State)> modStates = new List<(string ModName, string State)>();

            string selQuery = "SELECT ModName, State FROM Mods";

            using (SQLiteCommand command = new SQLiteCommand(selQuery, _connection)) 
            {
                using (SQLiteDataReader reader = command.ExecuteReader()) 
                {
                    while (reader.Read()) 
                    {
                        string modName = reader.IsDBNull(0) ? string.Empty : reader.GetString(0);
                        string state = reader.IsDBNull(1) ? string.Empty : reader.GetString(1);
                        modStates.Add((modName, state));
                    }
                }
            }

            return modStates;
        }

        public void UpdateModState(string modName, ModState state)
        {
            // Convert ModState enum value to string
            string stateString = state.ToString();

            // Use UpdateStateData method to update the state
            UpdateStateData(modName, stateString);
        }

    }
}
