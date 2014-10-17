using Nimble.Messages.Contracts;
using System;
using System.Reactive;
using Nimble.Core.Diagnostics;

namespace Messaging
{
    class Publisher
    {
        private IObserver<Node> _nodeObserver;

        public Publisher()
        {
            _nodeObserver = Observer.Create<Node>(n => { }).Trace();
        }
    }
}
