using System;
using System.Diagnostics;
using System.IO;

namespace PortListener
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                AppDomain.CurrentDomain.UnhandledException += UnhandledExceptionHandler;
                string exe = Path.GetFileName(Process.GetCurrentProcess().MainModule.FileName);
                Console.WriteLine(exe + " - Port Listener");
                Config config;
                if (!CommandLine.ParseArgs(args, out config))
                {
                    return;
                }
                Connection.Listen(config);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex);
                Environment.Exit(1);
            }
        }

        private static void UnhandledExceptionHandler(object sender, UnhandledExceptionEventArgs e)
        {
            Console.WriteLine("Error: " + e.ExceptionObject);
            Environment.Exit(2);
        }
    }
}