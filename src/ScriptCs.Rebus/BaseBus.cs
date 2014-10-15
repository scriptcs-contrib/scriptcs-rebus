using System;
using System.Collections.Concurrent;
using System.IO;
using System.Text;
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

        public abstract BaseBus Receive<T>(Action<T> action) where T : class;

        public abstract void Start();

        public abstract BaseBus UseLogging();

        protected void ShutDown()
        {
            if (SendBus != null) SendBus.Dispose();
            if (ReceiveBus != null) ReceiveBus.Dispose();
        }

        public ScriptConfiguration SendAScriptFile(string scriptFile)
        {
            //SendAScript(File.ReadAllText(scriptFile), namespaceName, dependencies);
            return new ScriptConfiguration(File.ReadAllText(scriptFile));
        }

        public ScriptConfiguration SendAScript(string script)
        {
            //Send(new Script { ScriptContent = script, Namespaces = namespaceName, Dependencies = dependencies });
            return new ScriptConfiguration(script);
        }
    }
}