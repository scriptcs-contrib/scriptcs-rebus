using System;
using System.Collections.Concurrent;
using Rebus;
using Rebus.Configuration;

namespace ScriptCs.Rebus
{
    public abstract class BaseBus
    {
        protected IBus SendBus;
        protected IBus ReceiveBus;
        protected readonly ConcurrentDictionary<string, Type> KnownTypes = new ConcurrentDictionary<string, Type>();
        public BuiltinContainerAdapter Container;

        public abstract void Send<T>(T message) where T : class;
        public abstract void SendAScript(string script, params string[] dependencies);
        public abstract BaseBus Receive<T>(Action<T> action) where T : class;
        public abstract void Start();
        public abstract BaseBus UseLogging();

        protected void ShutDown()
        {
            if (SendBus != null) SendBus.Dispose();
            if (ReceiveBus != null) ReceiveBus.Dispose();
        }

    }
}