using System;
using System.CodeDom;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq.Expressions;
using System.Runtime.InteropServices;
using System.Text;
using Rebus;
using Rebus.Configuration;
using ScriptCs.Rebus.Configuration;
using ScriptCs.Rebus.Scripts;

namespace ScriptCs.Rebus
{
    public abstract class BaseBus : IDisposable
    {
        protected internal IBus SendBus;
	    protected internal IBus ReceiveBus;

	    protected BuiltinContainerAdapter Container;
	    protected readonly ConcurrentDictionary<string, Type> KnownTypes = new ConcurrentDictionary<string, Type>();
	    protected internal string Endpoint { get; set; }
	    protected internal List<ScriptConfiguration> ScriptConfigurations = new List<ScriptConfiguration>();

	    public void RegisterHandler(Func<IHandleMessages> messageHandler)
	    {
		    Container.Register(messageHandler);
	    }

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

		//public CustomScriptConfiguration With
		//{
		//	get { return new CustomScriptConfiguration(new ScriptConfiguration(this, Endpoint), s => _customScripts.Add);}
		//}
			
	    public void Dispose()
	    {
		    ShutDown();
	    }
    }

	public class CustomScriptConfiguration
	{
	}
}