using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Web.Script.Serialization;
using System.ServiceProcess;

namespace BlackMamba.Billing.Background
{
    class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
            if (Environment.UserInteractive)
            {
                ProcessCommandLine(args);
            }
            else
            {
                RunService();
            }
        }

        private static void RunService()
        {
            ServiceBase[] ServicesToRun;
            ServicesToRun = new ServiceBase[]
            { 
                new BillingService() 
            };
            ServiceBase.Run(ServicesToRun);
        }

        private static void ProcessCommandLine(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("================================");
                Console.WriteLine("For help, please run : BlackMamba.Billing.Background -help");
                Console.WriteLine("Press Ctrl + C to exit!");
                Console.WriteLine("================================");
                RunInCommandLine();
                //Console.ReadLine();         
                return;
            }

            foreach (var x in args)
            {
                switch (x)
                {
                    case "-i":
                    case "-install":
                        InstallService();
                        return;
                    case "-u":
                    case "-uninstall":
                        UninstallService();
                        return;
                    case "-h":
                    case "-help":
                        ShowCommandHelp();
                        return;
                    case "-d":
                        RunInCommandLine();
                        return;
                    default:
                        Console.WriteLine("Unknown argument: {0}", x);
                        return;
                }
            }
        }

        private static void ShowCommandHelp()
        {
            Console.WriteLine(@"
================================
Usage: BlackMamba.Billing.Background [-i | -install | -u | -uninstall | -h | -help]

-i
-install
    install windows service
-u
-uninstall
    uninstall windows service
-h
-help
    show usage information
-d [date]
================================
");
        }

        private static void UninstallService()
        {
            var installer = new System.Configuration.Install.AssemblyInstaller(typeof(BillingServiceInstaller).Assembly, null);
            installer.UseNewContext = true;
            installer.Uninstall(null);
        }

        private static void InstallService()
        {
            var installer = new System.Configuration.Install.AssemblyInstaller(typeof(BillingServiceInstaller).Assembly, null);
            installer.UseNewContext = true;
            installer.Install(null);
        }

        private static void RunInCommandLine()
        {
            BillingService server = new BillingService();
            server.JobStart();
            Console.ReadLine();
        }
        
    }
}
