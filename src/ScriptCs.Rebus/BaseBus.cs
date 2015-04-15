using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Rebus;
using Rebus.Configuration;
using ScriptCs.Rebus.Configuration;

namespace ScriptCs.Rebus
{
    public abstract class BaseBus
    {
        protected internal IBus SendBus;
	    protected internal IBus ReceiveBus;

	    protected readonly ConcurrentDictionary<string, Type> KnownTypes = new ConcurrentDictionary<string, Type>();
	    protected internal string Endpoint { get; set; }
	    protected internal List<ScriptConfiguration> ScriptConfigurations = new List<ScriptConfiguration>();
		protected BuiltinContainerAdapter Container;

	    public abstract void Send<T>(T message) where T : class;

        public abstract BaseBus Receive<T>(Action<T> action) where T : class;

        public abstract void Start();

        public abstract BaseBus UseLogging();

        public virtual void ShutDown()
        {
            if (SendBus != null) SendBus.Dispose();
            if (ReceiveBus != null) ReceiveBus.Dispose();
        }

		//public ScriptConfiguration WithAScriptFile(string scriptFile)
		//{
		//	return new ScriptConfiguration(this, File.ReadAllText(scriptFile), Endpoint);
		//}

		//public ScriptConfiguration WithAScript(string script)
		//{
		//	return new ScriptConfiguration(this, script, Endpoint);
		//}

		public ScriptConfiguration With
		{
			get { return new ScriptConfiguration(this, Endpoint); }
		}
    }
}