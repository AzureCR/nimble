using System;
using System.Diagnostics;
using System.IO;

namespace Nimble.Core.Diagnostics
{
    public static class TraceObserverEx
    {
        public static IObserver<T> Trace<T>(this IObserver<T> observer, bool consoleTrace = true)
        {
            return new TracingObserver<T>(observer, consoleTrace);
        }

        class TracingObserver<T> : IObserver<T>
        {
            private IObserver<T> _observer;
            private readonly int _hashCode;
            private TextWriter _output;

            public TracingObserver(IObserver<T> observer, bool consoleTrace)
            {

                _output = consoleTrace ? Console.Out : TextWriter.Null;
                this._observer = observer;
                this._hashCode = observer.GetHashCode();
            }

            public void OnCompleted()
            {
                string message = string.Format("Completed : {0} ", this.GetType().Name);
                Debug.WriteLine(message);
                _output.WriteLine(message);
                _observer.OnCompleted();
            }

            public void OnError(Exception error)
            {
                string message = string.Format("OnError : {0} \n Expcetion : {1}", this.GetType().Name, error);
                Debug.WriteLine(message);
                _output.WriteLine(message);
                _observer.OnError(error);
            }

            public void OnNext(T value)
            {
                string message = string.Format("OnNext : {0} \t Value: {1}", this.GetType().Name, value);
                Debug.WriteLine(message);
                _output.WriteLine(message);
                _observer.OnNext(value);
            }
        }
    }
}
