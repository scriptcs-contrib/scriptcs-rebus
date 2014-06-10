using System;
using System.Collections.Concurrent;
using Rebus;

namespace ScriptCs.Rebus
{
    public abstract class BaseBus
    {
        protected IBus _sendBus;
        protected IBus _receiveBus;
        protected readonly ConcurrentDictionary<string, Type> KnownTypes = new ConcurrentDictionary<string, Type>();

        public abstract void Send<T>(T message) where T : class;
        public abstract BaseBus Receive<T>(Action<T> action) where T : class;
        public abstract void Start();
        public abstract BaseBus UseLogging();

        protected void ShutDown()
        {
            if (_sendBus != null) _sendBus.Dispose();
            if (_receiveBus != null) _receiveBus.Dispose();
        }

    }
}