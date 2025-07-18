using Get_Last_Commit_Time;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using Newtonsoft.Json.Linq;
using System;
using System.Globalization;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Mail;
using System.Security.Cryptography.X509Certificates;
using System.Text.Json;
using System.Threading.Tasks;
using static System.Net.WebRequestMethods;
using System.Net;
using System.Net.Mail;

class Program
{
    static string systemDrive = Path.GetPathRoot(Environment.GetFolderPath(Environment.SpecialFolder.System));
    static string JsonStorePath = Path.Combine(systemDrive, "Notifier", "JsonData");
    static string smtpCredentialPath = Path.Combine(systemDrive, "Notifier", "smtpCredential.json");
    static string oldEventsJsonPath = Path.Combine(JsonStorePath, "oldEvents.json");
    static string newEventJsonPath = Path.Combine(JsonStorePath, "newEvents.json");
    static string metaDataJsonPath = Path.Combine(systemDrive, "Notifier", "eventsMetadata.json");
    static string configDataJsonPath = Path.Combine(systemDrive, "Notifier", "config.json");
    static ConfigModel ConfigData;

    public class GoogleSheetData
    {
        public DateTime Timestamp { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public bool SubscribeStatus { get; set; }
    }
    public static async Task Main()
    {

        string configDataJson = ReadFileContent(configDataJsonPath);
        ConfigData = JsonSerializer.Deserialize<ConfigModel>(configDataJson);

        if (!System.IO.File.Exists(oldEventsJsonPath))
        {
            Directory.CreateDirectory(JsonStorePath);
            string jsonString = GetJsonFromGitHub();
            Console.WriteLine(jsonString);
            List<EventModel> updatedJsonEvents = JsonSerializer.Deserialize<List<EventModel>>(jsonString);
            await JsonStorageHelper.SaveEventsAsync(updatedJsonEvents, oldEventsJsonPath);
        }
        // make these as seperate method and run in regular interval
        MetaData metaData = await JsonStorageHelper.GetConfigAsync(metaDataJsonPath);
        DateTime lastStoredCommitTime = metaData.LastCommitTime;
        DateTime currentCommitTime = await GetLastUpdateTime();
        if (currentCommitTime > lastStoredCommitTime)
        {
            Console.WriteLine("New commit detected!");
            List<EventModel> newEvents =  GetNewEvents();


            List<GoogleSheetData> userData = ReadSheet();
            foreach (var e in newEvents)
            {
                string htmlBody = MailContentManager.GenerateEventHtml(e);
                Console.WriteLine(SendMail(userData, $"Community Event Update: {e.eventName} on {e.eventDate}", htmlBody));
            }

            string jsonString = GetJsonFromGitHub();

            // update latest json
            Console.WriteLine(jsonString);
            List<EventModel> updatedJsonEvents = JsonSerializer.Deserialize<List<EventModel>>(jsonString);
            await JsonStorageHelper.SaveEventsAsync(updatedJsonEvents, oldEventsJsonPath);

            //update last commit time
            metaData.LastCommitTime = currentCommitTime;
            JsonStorageHelper.SaveEventsAsync(metaData, metaDataJsonPath);


            // Fetch latest JSON and store new commit time

        }
        else
        {
            Console.WriteLine("No new commit.");
        }
    }

        

public static bool SendMail(List<GoogleSheetData> usersData, string subject, string htmlBody)
{
        string fromName = ConfigData.fromName;
        string appPassword = ConfigData.appPassword;  // Use App Password here

        string fromEmail = ConfigData.fromEmail;

        try
        {
            var fromAddress = new MailAddress(fromEmail, fromName);
            var fromPassword = appPassword;

            using (var smtp = new SmtpClient
            {
                Host = "smtp.gmail.com",
                Port = 587,
                EnableSsl = true,
                Credentials = new NetworkCredential(fromAddress.Address, fromPassword)
            })
            {
                foreach (var userData in usersData)
                {
                    using (var message = new MailMessage(fromAddress, new MailAddress(userData.Email))
                    {
                        Subject = subject,
                        Body = htmlBody,
                        IsBodyHtml = true
                    })
                    {
                        smtp.Send(message);
                        Console.WriteLine($"send {subject} to {userData.Email}");
                    }
                }
            }
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            return false;
        }
    }



public static List<GoogleSheetData> ReadSheet()
    {
        List<GoogleSheetData> usersData = new List<GoogleSheetData>();
        string[] Scopes = { SheetsService.Scope.SpreadsheetsReadonly };
        string applicationName = ConfigData.applicationName;
        string spreadsheetId = ConfigData.spreadsheetId;
        var sheetRange = ConfigData.sheetRange;

        string ServiceAccountJsonPath = ReadFileContent(smtpCredentialPath);

        var credential = GoogleCredential.FromFile(smtpCredentialPath)
            .CreateScoped(Scopes);

        var service = new SheetsService(new BaseClientService.Initializer()
        {
            HttpClientInitializer = credential,
            ApplicationName = applicationName,
        });

        var request = service.Spreadsheets.Values.Get(spreadsheetId, sheetRange);
        var response = request.Execute();
        var values = response.Values;

        if (values == null || values.Count <= 1)
        {
            Console.WriteLine("No data found or only header row present.");
            return usersData;
        }

        foreach (var row in values.Skip(1))
        {
            if (row.Count >= 4)
            {
                try
                {
                    if (!row[3].ToString().Contains("Subscribe"))
                    {
                        continue;
                    }
                    var googleSheetData = new GoogleSheetData
                    {
                        Timestamp = DateTime.ParseExact(row[0].ToString(), "dd/MM/yyyy HH:mm:ss", CultureInfo.InvariantCulture),
                        Name = row[1].ToString(),
                        Email = row[2].ToString(),
                        SubscribeStatus = row[3].ToString().Contains("Subscribe")
                    };

                    
                    usersData.Add(googleSheetData);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error parsing row: {string.Join(",", row)} - {ex.Message}");
                }
            }
        }

        return usersData;
    }


    public static List<EventModel> GetNewEvents()
    {
        try
        {
            var oldEventsJson = System.IO.File.ReadAllText(oldEventsJsonPath);
            var oldEvents = JsonSerializer.Deserialize<List<EventModel>>(oldEventsJson) ?? new List<EventModel>();

            var newEventsJson = GetJsonFromGitHub();
            var newEvents = JsonSerializer.Deserialize<List<EventModel>>(newEventsJson) ?? new List<EventModel>();

            //var newEvents = JsonSerializer.Deserialize<List<EventModel>>(System.IO.File.ReadAllText(newEventJsonPath)); // for internal testing

            var addedEvents = newEvents
                .Where(newEv => !oldEvents.Any(oldEv =>
                    string.Equals(oldEv.eventName, newEv.eventName, StringComparison.OrdinalIgnoreCase) &&
                    oldEv.eventDate == newEv.eventDate))
                .ToList();

            return addedEvents;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error while fetching new events: {ex.Message}");
            return new List<EventModel>();
        }
    }




    public static string GetJsonFromGitHub()
    {
        string rawJsonApi = ConfigData.rawJsonApi;
        using (var client = new HttpClient())
        {
            client.DefaultRequestHeaders.UserAgent.ParseAdd("request"); // GitHub needs a User-Agent

            var response = client.GetAsync(rawJsonApi).GetAwaiter().GetResult();
            response.EnsureSuccessStatusCode();

            var json = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            return json;
        }
    }


    public static async Task<DateTime> GetLastUpdateTime()
    {
        try
        {
            var eventJsonUrl = ConfigData.eventJsonUrl;

            using var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("CSharpApp", "1.0"));

            var response = await httpClient.GetStringAsync(eventJsonUrl);
            var commitsArray = JArray.Parse(response);

            if (commitsArray.Count > 0)
            {
                var utcDateString = commitsArray[0]["commit"]?["committer"]?["date"]?.ToString();

                if (DateTime.TryParse(utcDateString, out DateTime utcDate))
                {
                    return ConvertUtcToIst(utcDate);
                }
                else
                {
                    Console.WriteLine("Unable to parse commit date.");
                }
            }
            else
            {
                Console.WriteLine("No commits found for this file.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error while fetching last update time: {ex.Message}");
        }

        return default;
    }

    public static string ReadFileContent(string filePath)
    {
        try
        {
            if (System.IO.File.Exists(filePath))
            {
                return System.IO.File.ReadAllText(filePath);
            }
            else
            {
                Console.WriteLine($"File not found: {filePath}");
                return string.Empty;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error reading file: {ex.Message}");
            return string.Empty;
        }
    }


    private static DateTime ConvertUtcToIst(DateTime utcDate)
    {
        var istZone = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
        return TimeZoneInfo.ConvertTimeFromUtc(utcDate, istZone);
    }
}
