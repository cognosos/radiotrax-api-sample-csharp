using System;
using System.IO;
using System.Collections.Specialized;
using System.Net;
using Newtonsoft.Json.Linq;
                    
public class Program
{
    static var user = "USERNAME";
    static var password = "PASSWORD";
    static var appcode = "APPCODE";
    static var apiRoot = "https://api.cognosos.net"

    static String homedir = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

    static String lastRunFilename() => Path.Combine(homedir, "cognosos_integration_lastseen.txt");
    static String runLog() => Path.Combine(homedir, "cognosos_integration_log.txt");

    public static DateTimeOffset getLastSeen() {
        var s = File.ReadAllText(lastRunFilename(), Encoding.UTF8);
        return DateTimeOffset.Parse(s);
    }

    public static void writeLastSeen(DateTimeOffset lastSeen) {
        var txt = lastSeen.ToString("o") + "\n");
        File.WriteAllText(lastRunFilename(), text)
    }

    public static void Main()
    {    
        Console.WriteLine("Logging to {0}", runLog());
        using (var log = new StreamWriter(runLog()))
        {
            var now = DateTimeOffset.Now;   
            log.WriteLine("Starting run at {0}", now);
            DateTimeOffset prevLastSeen = DateTimeOffset.Now.AddDays(-3);
            try {
                log.WriteLine("reading last run time from {0}", lastRunFilename());
                prevLastSeen = getLastSeen();
            } 
            catch (Exception e) {
                log.WriteLine($"Unable to load previous run time from {lastRunFilename} - {e}");
                log.WriteLine("Using default query period of 3 days ago {0}", prevLastSeen);
            }
            DateTimeOffset lastSeenGpsTimestamp = prevLastSeen;
            var cred = new NetworkCredential(user, password);
            var timeQueryMilliseconds = prevLastSeen.ToUnixTimeMilliseconds().ToString();
            log.WriteLine("Querying active node status at {0}, start date param {1}", now, timeQueryMilliseconds);
            var apiUrl = apiRoot + "/node/activeNodeStatus";
            var qs = new NameValueCollection();
            qs.Add("start_gps_date", timeQueryMilliseconds);
            qs.Add("application_code", appcode);
            var client = new WebClient { Credentials = cred, QueryString = qs };
            log.WriteLine("Querying {0}", apiUrl);
            var jsonResponse = client.DownloadString(apiUrl);
            var vehicles = JArray.Parse(jsonResponse);
            log.WriteLine($"Parsed {vehicles.Count} vehicles from API response");
            foreach(JObject v in vehicles) {
                try {
                    var assetId = (string)(v["asset_identifier"]);
                    log.WriteLine($"Got asset {assetId}");
                    var lat = (double?)(v["latitude"]);
                    var lon = (double?)(v["longitude"]);
                    var lastMessage = (DateTimeOffset?)(v["last_message_date"]);
                    var lastGps = (DateTimeOffset?)(v["last_gps_message_date"]);
                    log.WriteLine($"With last gps date {lastGps}");
                    var zones = (string)(v["current_zone"]).Split(";");
                    foreach(var current_zone in zones) {
                        if(lat == null || lon == null) {
                            log.WriteLine("{0} has no current location as of {1}", assetId, lastMessage);
                        } else {                
                            log.WriteLine("{0} in {4} at {1} {2} as of {3}", assetId, lat, lon, lastGps, current_zone);
                        }
                    }
                }
                catch(Exception e) {
                    log.WriteLine($"Failed to parse vehicle: {e}");
                }
            }
            log.WriteLine($"Writing last-run timestamp {lastSeenGpsTimestamp}");
            writeLastSeen(lastSeenGpsTimestamp);
        }
    }
    
}


