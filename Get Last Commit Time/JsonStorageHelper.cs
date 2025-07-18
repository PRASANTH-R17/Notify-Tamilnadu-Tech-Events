using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Get_Last_Commit_Time
{
    public class ConfigModel
    {
        // Json from Github
        public string eventJsonUrl { get; set; }
        public string rawJsonApi { get; set; }

        // Mail Configuration
        public string fromName { get; set; }
        public string appPassword { get; set; }
        public string fromEmail { get; set; }

        // Spreadsheet services configuration
        public string applicationName { get; set; }
        public string spreadsheetId { get; set; }
        public string sheetRange { get; set; }
    }

    public class EventModel
    {
        public string eventName { get; set; }
        public string eventDescription { get; set; }
        public string eventDate { get; set; }
        public string eventTime { get; set; }
        public string eventVenue { get; set; }
        public string eventLink { get; set; }
        public string location { get; set; }
        public string communityName { get; set; }
        public string communityLogo { get; set; }

    }
    public class MetaData
    {
        public DateTime LastCommitTime { get; set; }
        public string ApiUrl { get; set; }
    }




    class JsonStorageHelper
    {
        public static async Task SaveEventsAsync<T>(T data, string filePath)
        {
            try
            {
                Console.WriteLine(filePath);
                using (var fs = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    await JsonSerializer.SerializeAsync(fs, data, new JsonSerializerOptions { WriteIndented = true });
                }
                Console.WriteLine("Data saved successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error while saving data: {ex.Message}");
            }
        }

        public static async Task<MetaData> GetConfigAsync(string configFilePath)
        {

            using (var stream = File.OpenRead(configFilePath))
            {
                return await JsonSerializer.DeserializeAsync<MetaData>(stream)
                       ?? new MetaData();
            }
            // return new ConfigModel();
        }

        /*public static async Task<bool> SaveEventsAsync(List<EventModel> events, string filePath)
        {
            try
            {
                if (!File.Exists(filePath))
                {
                    using (File.Create(filePath))
                    {
                        Console.WriteLine("File not found. Creating new file...");
                    }
                }
                else
                {
                    Console.WriteLine("File exists. Overwriting...");
                }

                string jsonData = JsonSerializer.Serialize(events, new JsonSerializerOptions { WriteIndented = true });
                await File.WriteAllTextAsync(filePath, jsonData);
                Console.WriteLine("Events saved successfully.");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error while saving events: {ex.Message}");
                return false;
            }
        }*/
    }

}
