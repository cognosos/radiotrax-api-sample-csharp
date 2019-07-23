using System;
using System.Text;
using System.IO;
using System.Collections.Specialized;
using System.Net;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

/// <summary>
/// Asset update skeleton. This class fetches all assets with a message date more recent than the latest date seen in the last run.
/// Running this script repeatedly should fetch all assets that have received a message, without gaps. 
/// Extend this class and override HandleAssetStats() to do your own processing. 
/// </summary>
public abstract class CognososAssetUpdateBase
{
    protected StreamWriter log = new StreamWriter(Pathnames.runLog, true);

    public abstract void HandleAssetStatus(CognososAssetStatus v);

    public CognososConfig GetConfig()
    {
        var s = File.ReadAllText(Pathnames.configFilename, Encoding.UTF8);
        var v = JsonConvert.DeserializeObject<CognososConfig>(s);
        return v;
    }

    public DateTimeOffset GetLastSeen()
    {
        var s = File.ReadAllText(Pathnames.lastRunFilename, Encoding.UTF8);
        return DateTimeOffset.Parse(s);
    }

    public void WriteLastSeen(DateTimeOffset lastSeen)
    {
        var txt = lastSeen.ToString("o") + "\n";
        File.WriteAllText(Pathnames.lastRunFilename, txt, Encoding.UTF8);
    }


    public void Run()
    {
        Console.WriteLine($"Logging to {Pathnames.runLog}");
        using (var log = this.log)
        {
            var config = GetConfig();
            var now = DateTimeOffset.Now;
            log.WriteLine($"Starting run at {now}");

            DateTimeOffset prevLastSeen = DateTimeOffset.Now.AddDays(-3);
            try
            {
                log.WriteLine($"reading last run time from {Pathnames.lastRunFilename}");
                prevLastSeen = GetLastSeen();
            }
            catch (Exception e)
            {
                log.WriteLine($"Unable to load previous run time from {Pathnames.lastRunFilename} - {e}");
                log.WriteLine($"Using default query period of 3 days ago {prevLastSeen}");
            }
            DateTimeOffset lastSeenGpsTimestamp = prevLastSeen;

            log.WriteLine($"Querying active node status at {now}, start date param {prevLastSeen}");
            var apiUrl = config.apiroot + "/node/activeNodeStatus";
            var client = new WebClient
            {
                Credentials = new NetworkCredential(config.username, config.password),
                QueryString = new NameValueCollection
                {
                    { "start_gps_date", prevLastSeen.ToUnixTimeMilliseconds().ToString() },
                    { "end_gps_date", now.ToUnixTimeMilliseconds().ToString() },
                    { "application_code", config.appcode }
                }
            };

            log.WriteLine("Querying {0}", apiUrl);
            var jsonResponse = client.DownloadString(apiUrl);

            var vehicles = JArray.Parse(jsonResponse);
            log.WriteLine($"Parsed {vehicles.Count} vehicles from API response");

            foreach (JObject j in vehicles)
            {
                try
                {
                    var v = j.ToObject<CognososAssetStatus>();
                    log.WriteLine($"Got asset {v.asset_identifier}");
                    log.WriteLine($"With last gps date {v.last_gps_message_date}");
                    if (v.last_gps_message_date.HasValue && v.last_gps_message_date.Value > lastSeenGpsTimestamp)
                    {
                        lastSeenGpsTimestamp = v.last_gps_message_date.Value;
                    }
                    HandleAssetStatus(v);
                }
                catch (Exception e)
                {
                    log.WriteLine($"Failed to parse vehicle: {e}");
                }
            }

            log.WriteLine($"Writing last-run timestamp {lastSeenGpsTimestamp}");
            WriteLastSeen(lastSeenGpsTimestamp);
        }


    }


}

