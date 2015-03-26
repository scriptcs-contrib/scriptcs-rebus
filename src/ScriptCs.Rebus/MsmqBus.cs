using System;
using System.Globalization;
using System.Reflection;
using Rebus;
using Rebus.Configuration;
using Rebus.Logging;
using Rebus.Serialization.Json;
using Rebus.Transports.Msmq;
using ScriptCs.Rebus.Configuration;
using ScriptCs.Rebus.Scripts;

namespace ScriptCs.Rebus
{
    public class MsmqBus : BaseBus
    {
        private Action<LoggingConfigurer> _loggingConfigurer;

        public MsmqBus(string endpoint)
        {
	        Endpoint = endpoint;
	        Container = new BuiltinContainerAdapter();
            _loggingConfigurer = configurer => configurer.None();
        }

	    public override void Send<T>(T message)
        {
            Guard.AgainstNullArgumentIfNullable("message", message);

	        var isAScript = message.GetType() == typeof(IExecutionScript) || message.GetType().BaseType == typeof(IExecutionScript);
	        if (SendBus == null)
	        {
		        ConfigureSendBus(isAScript);
	        }

		    Guard.AgainstNullArgument("_sendBus", SendBus);
		    
			// Add header information
		    if (isAScript)
		    {
			    SendBus.AttachHeader(message, "transport", "MSMQ");
		    }

		    Console.Write("Sending message of type {0}...", message.GetType().Name);
            try
            {
                SendBus.Advanced.Routing.Send(Endpoint, message);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            finally
            {
				ShutDown();
            }

            Console.WriteLine("sent.");

	        ShutDown();
        }

	    public override BaseBus Receive<T>(Action<T> action)
        {
            Guard.AgainstNullArgument("action", action);

            KnownTypes[typeof(T).Name] = typeof(T);
	        Container.Handle(action);

            return this;
        }

	    public override void Start()
        {
            if (ReceiveBus == null)
            {
                ConfigureReceiveBus();
            }

            Console.WriteLine("Awaiting messsage on {0}...", Endpoint);
        }

	    public override BaseBus UseLogging()
        {
            _loggingConfigurer = configurer => configurer.Console();

            return this;
        }

	    private void ConfigureSendBus(bool isAScript)
        {
	        Action<RebusTransportConfigurer> transportConfig;
	        if (!isAScript)
	        {
		        transportConfig =  configurer => configurer.UseMsmqInOneWayClientMode();
	        }
	        else
	        {
		        transportConfig =
			        configurer =>
				        configurer.UseMsmq(string.Format("{0}.reply", Endpoint),
					        string.Format("{0}.reply.error", Endpoint));
	        }


	        SendBus = Configure.With(Container)
				.Logging(_loggingConfigurer)
		        .Serialization(serializer => serializer.UseJsonSerializer()
			        .AddNameResolver(
				        x => x.Assembly.GetName().Name.Contains("ℛ")
					        ? new TypeDescriptor("ScriptCs.Compiled", x.Name)
					        : null))
		        .Transport(transportConfig)
		        .CreateBus()
		        .Start();

        }

	    private void ConfigureReceiveBus()
	    {
		    ReceiveBus = Configure.With(Container)
			    .Logging(_loggingConfigurer)
			    .Serialization(serializer => serializer.UseJsonSerializer()
				    .AddTypeResolver(
					    x =>
						    x.AssemblyName == "ScriptCs.Compiled" ? KnownTypes[x.TypeName] : null))
			    .Transport(
				    configurer =>
					    configurer.UseMsmq(Endpoint, string.Format("{0}.error", Endpoint)))
			    .CreateBus()
			    .Start();
	    }
    }
}