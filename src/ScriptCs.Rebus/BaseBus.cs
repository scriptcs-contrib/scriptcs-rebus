using System;
using System.Collections.Concurrent;
using System.IO;
using System.Text;
using Rebus;
using Rebus.Configuration;
using ScriptCs.Rebus.Configuration;
using ScriptCs.Rebus.Scripts;

namespace ScriptCs.Rebus
{
    public abstract class BaseBus : IDisposable
    {
        protected IBus SendBus;
	    protected IBus ReceiveBus;

	    public BuiltinContainerAdapter Container;
	    protected readonly ConcurrentDictionary<string, Type> KnownTypes = new ConcurrentDictionary<string, Type>();
	    protected internal string Endpoint { get; set; }

	    public abstract void RegisterHandler(Func<IHandleMessages> messageHandler);

	    public abstract void Send<T>(T message) where T : class;

        public abstract BaseBus Receive<T>(Action<T> action) where T : class;

        public abstract void Start();

        public abstract BaseBus UseLogging();

        protected void ShutDown()
        {
            if (SendBus != null) SendBus.Dispose();
            if (ReceiveBus != null) ReceiveBus.Dispose();
        }

        public ScriptConfiguration WithAScriptFile(string scriptFile)
        {
            return new ScriptConfiguration(this, File.ReadAllText(scriptFile), Endpoint);
        }

        public ScriptConfiguration WithAScript(string script)
        {
            return new ScriptConfiguration(this, script, Endpoint);
        }

	    public void Dispose()
	    {
		    ShutDown();
	    }
    }
}