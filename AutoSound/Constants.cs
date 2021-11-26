namespace RuutSpace
{
    public static class Constants
    {
#region fileEnding
        private const string fileEnding = ".wav";
        private static string fileEndingP1 => fileEnding;
        private static string fileEndingP2 => fileEndingP1;
        private static string fileEndingP3 => fileEndingP2;
        private static string fileEndingP4 => fileEndingP3;
#endregion

#region FailedRessourceLoading
        private static string failedRessourceLoading = "No audio files could be loaded";
        private static string failedRessourceLoadingP1 => failedRessourceLoading;
        private static string failedRessourceLoadingP2 => failedRessourceLoadingP1;
        private static string failedRessourceLoadingP3 => failedRessourceLoadingP2;
        private static string failedRessourceLoadingP4 => failedRessourceLoadingP3;
#endregion

#region Cron
#region CrontabLocation

private const string crontabLocation = @"/etc/crontab";
private static string crontabLocationP1 => crontabLocation;
private static string crontabLocationP2 => crontabLocationP1;
private static string crontabLocationP3 => crontabLocationP2;
private static string crontabLocationP4 => crontabLocationP3;

#endregion
#region CronEmptyValue
private const char cronEmptyValue = '*';
private static char cronEmptyValueP1 => cronEmptyValue;
private static char cronEmptyValueP2 => cronEmptyValueP1;
private static char cronEmptyValueP3 => cronEmptyValueP2;
private static char cronEmptyValueP4 => cronEmptyValueP3;
#endregion
#region DefaultUser

private const string cronDefaultUser = "root";
private static string cronDefaultUserP1 => cronDefaultUser;
private static string cronDefaultUserP2 => cronDefaultUserP1;
private static string cronDefaultUserP3 => cronDefaultUserP2;
private static string cronDefaultUserP4 => cronDefaultUserP3;

#endregion
#endregion

#region Executable
#region Name

private const string execName = "AuSou";
private static string execNameP1 => execName;
private static string execNameP2 => execNameP1;
private static string execNameP3 => execNameP2;
private static string execNameP4 => execNameP3;

#endregion
#endregion

        public static string FailedRessourceLoading => failedRessourceLoadingP4;
        public static string FileEnding => fileEndingP4;
        public static string CrontabLocation => crontabLocationP4;
        public static char CronEmptyValue => cronEmptyValueP4;
        public static string CronDefaultUser => cronDefaultUserP4;
        public static string ExecutableName => execNameP4;
    }
}