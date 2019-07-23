using System;
using System.IO;

public static class Pathnames {
    public static string homedir = 
        Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

    public static string lastRunFilename => 
        Path.Combine(homedir, "cognosos_integration_lastseen.txt");

    public static string configFilename => 
        Path.Combine(homedir, "cognosos_config.json");

    public static string runLog => 
        Path.Combine(homedir, "cognosos_integration_log.txt");
}

