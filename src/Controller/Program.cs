using System;
using System.Diagnostics;
using Microsoft.Owin.Hosting;
using Nimble.Controller.App;
using Nimble.Core.Config;

namespace Nimble.Controller
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            const string baseUrl = Settings.ControllerBaseAddress;
            using (WebApp.Start<Startup>(baseUrl))
            {
                Console.WriteLine("Press Enter to quit.");
                Process.Start(baseUrl);
                Console.ReadKey();
            }            
        }
    }
}
