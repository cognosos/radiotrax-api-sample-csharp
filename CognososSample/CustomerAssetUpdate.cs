


public class CustomerAssetUpdate : CognososAssetUpdateBase {


    /// <summary>
    /// Fill in your update handling code here.  For example, you might update an internal database.
    /// </summary>
    /// <param name="v"></param>
    override public void HandleAssetStatus(CognososAssetStatus v)
    {
        foreach (var current_zone in v.zones)
        {
            if (v.latitude.HasValue && v.longitude.HasValue)
            {
                log.WriteLine($"{v.asset_identifier} in {v.current_zone} at {v.latitude} {v.longitude} as of {v.last_gps_message_date}");
            }
            else
            {
                log.WriteLine($"{v.asset_identifier} has no current location as of {v.last_gps_message_date}");
            }
        }
    }

}



