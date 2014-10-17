using Nimble.Messages.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Text;
using System.Threading.Tasks;
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
