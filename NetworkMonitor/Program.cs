using System;
using System.Diagnostics;
using System.Threading;

namespace NetworkMonitor
{
    internal class Program
    {
        private static readonly string Url = "www.baidu.com";
        private static readonly int MaxRetries = 10;
        private static readonly int RetryInterval = 60;
        private static readonly DayOfWeek RebootDayofWeek = DayOfWeek.Tuesday;
        private static readonly int RebootHour = 2;
        private static readonly int RebootMinute = 0;

        private static DateTime RebootTime;
        private static int checkNetworkFailTimes = 0;

        private static void Main(string[] args)
        {
            Log("Program starts");
            GetRebootTime();
            while (true)
            {
                if (TaskCheckNetwork())
                {
                    Reboot();
                    break;
                }
                else if (TaskRebootWeekly())
                {
                    Reboot();
                    break;
                }
                else
                {
                    Thread.Sleep(RetryInterval * 1000);
                }
            }
            Log("Program exits");
        }

        private static void GetRebootTime()
        {
            DateTime startDate = DateTime.Today;
            DateTime rebootDate = startDate.AddDays(RebootDayofWeek > startDate.DayOfWeek ?
                RebootDayofWeek - startDate.DayOfWeek : RebootDayofWeek + 7 - startDate.DayOfWeek);
            RebootTime = new DateTime(rebootDate.Year, rebootDate.Month, rebootDate.Day, RebootHour, RebootMinute, 0);
            Log("Current date is " + startDate.ToString("yyyy-MM-dd"));
            Log("Next reboot time is " + RebootTime.ToString("yyyy-MM-dd HH:mm:ss"));
        }

        private static bool TaskRebootWeekly()
        {
            return DateTime.Compare(DateTime.Now, RebootTime) > 0;
        }

        private static bool TaskCheckNetwork()
        {
            if (CheckNetwork(Url))
            {
                checkNetworkFailTimes = 0;
            }
            else
            {
                checkNetworkFailTimes++;
                Log(string.Format("Check network fails -> {0} / {1}", checkNetworkFailTimes, MaxRetries));
            }
            return checkNetworkFailTimes >= MaxRetries;
        }

        private static void Reboot()
        {
            RunCmd("shutdown -r -t 10");
            Trace.WriteLine("Computer will reboot in 10s");
        }

        private static bool CheckNetwork(string url)
        {
            (string output, string error) = RunCmd("ping " + url);
            return output.Contains("TTL=");
        }

        private static (string Output, string Error) RunCmd(string cmd)
        {
            Process p = new Process();
            p.StartInfo.FileName = "cmd.exe";
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.RedirectStandardInput = true;
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.RedirectStandardError = true;
            p.StartInfo.CreateNoWindow = true;
            p.Start();
            p.StandardInput.WriteLine(cmd);
            p.StandardInput.WriteLine("exit");
            string output = p.StandardOutput.ReadToEnd();
            string error = p.StandardError.ReadToEnd();
            p.Close();
            return (output, error);
        }

        private static void Log(string str)
        {
            Trace.WriteLine(DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss.fff") + " " + str);
        }
    }
}
