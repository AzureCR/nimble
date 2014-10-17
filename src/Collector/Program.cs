using Microsoft.Diagnostics.Tracing;
using Microsoft.Diagnostics.Tracing.Session;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Tx.Windows;
using System.Reactive.Linq;
using System.Reactive.Disposables;

namespace Controller
{
    class Program
    {
        static void Main()
        {
            Guid g = Guid.Parse("1877c56b-c362-5df7-a1ba-f07146a7c3d6");
            var disposable = CaptureManifests(g);
            Console.WriteLine("Press Q to quit.");
            Console.ReadLine();
            disposable.Dispose();
        }

        public static IDisposable CaptureManifests(Guid provider)
        {
            var sessionName = "nimble";
            var all = TraceEventSession.GetActiveSessionNames();
            Console.WriteLine("Active sessions : ");
            foreach (var name in all)
            {
                Console.WriteLine("\t" + name);
            }

            var traceSessoion = new TraceEventSession(sessionName, TraceEventSessionOptions.Create);
            traceSessoion.EnableProvider(Microsoft.Diagnostics.Tracing.Parsers.KernelTraceEventParser.ProviderGuid);
            traceSessoion.DisableProvider(Microsoft.Diagnostics.Tracing.Parsers.KernelTraceEventParser.ProviderGuid);

            var manifests = TraceManifestObservable.GetManifests(sessionName);

            var disposable = manifests.Select(manifest =>
            {
                var manifestDictionary = ManifestParser.Parse(manifest);
                Console.WriteLine("-----------------------------------");

                foreach (var value in manifestDictionary.Values)
                {
                    Console.WriteLine(value);
                }
                Console.WriteLine("-----------------------------------");
                return manifestDictionary;
            }).Subscribe(_ => { }, ex =>
            {
                Console.WriteLine("Exception :" + ex.Message);
            },
            () => { Console.WriteLine("Session completed"); });

            Task.Run(() =>
            {
                Thread.Sleep(5000);
                Console.WriteLine("Provider Enabled.");
                traceSessoion.EnableProvider(provider);
                Thread.Sleep(10000);
                Console.WriteLine("Capture session called.");
                traceSessoion.CaptureState(provider);
                traceSessoion.Dispose();
            });

            return Disposable.Create(() =>
            {
                disposable.Dispose();
                traceSessoion.Stop();
            });
        }
    }
}
