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
        private static void Main(string[] args)
        {
            Log("Program starts");
            int i = 0;
            while (true)
            {
                if (CheckNetwork(Url))
                {
                    i = 0;
                }
                else
                {
                    i++;
                    Log(string.Format("Check network fails -> {0} / {1}", i, MaxRetries));
                }
                if (i >= MaxRetries)
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
        private static void Reboot()
        {
            RunCmd("shutdown -r -t 10");
            Trace.WriteLine("Computer will reboot in 10s");
        }
        private static bool CheckNetwork(string url)
        {
            (string output, string error) = RunCmd("ping " + url);
            if (output.Contains("TTL="))
            {
                return true;
            }
            else
            {
                return false;
            }
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
