using System;
using System.Linq;



/// <summary>
/// This class represents one record from the activeNodeStatus endpoint
/// </summary>
public class CognososAssetStatus {
    public double? latitude;
    public double? longitude;
    public DateTimeOffset? last_gps_message_date;
    public DateTimeOffset? last_message_date;
    public string asset_identifier;
    public long? device_id;
    public string current_zone;

    public string[] zones => 
        current_zone != null 
        ? current_zone.Split(';').Where(zn => zn != "").ToArray() 
        : new string[] { } ;
}

