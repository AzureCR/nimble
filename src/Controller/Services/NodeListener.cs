using Nimble.Messages.Contracts;
using System;

namespace Nimble.Controller.Services
{
    public class NodeListener
    {
        public IObservable<Node> OnConnect()
        {
            throw new NotImplementedException();
        }

        public IObservable<Node> OnDisconnect()
        {
            throw new NotImplementedException();
        }
    }
}
