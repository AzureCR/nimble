using Microsoft.Diagnostics.Tracing;
using System;
using System.Reactive.Linq;
using System.Threading;

namespace EventProducer
{
    class Program
    {
        static void Main(string[] args)
        {

            // 50 ms Staggered start and stops of of 100ms operations starting 50ms
            for (int i = 0; i < 2; i++)
            {
                Observable.Interval(TimeSpan.FromMilliseconds(100)).Subscribe(_ =>
                {
                    var activity = Guid.NewGuid();
                    EventProducerEventSource.Events.OperationStart(activity);
                    Thread.SpinWait(1000);
                    EventProducerEventSource.Events.OperationStop();
                });

                Thread.Sleep(50);

            }

            Console.WriteLine("Press Any key to terminate.");
            Console.ReadKey();
        }

        [EventSource(Name = "EventProducerEventSource")]
        public sealed class EventProducerEventSource : EventSource
        {
            static EventProducerEventSource()
            {
                var guid = EventSource.GetGuid(typeof(EventProducerEventSource));
                Console.WriteLine("Provider : " + guid);
            }

            public static EventProducerEventSource Events = new EventProducerEventSource();

            public void OperationStart(Guid activityId)
            {
                SetCurrentThreadActivityId(activityId);
                WriteEvent(1);
            }

            public void OperationStop()
            {
                WriteEvent(2);
            }

            public static EventProducerEventSource Log = new EventProducerEventSource();
        }
    }
}
