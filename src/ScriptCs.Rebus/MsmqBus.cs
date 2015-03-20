using System;
using System.Globalization;
using System.Reflection;
using Rebus;
using Rebus.Configuration;
using Rebus.Logging;
using Rebus.Serialization.Json;
using Rebus.Transports.Msmq;
using ScriptCs.Rebus.Scripts;

namespace ScriptCs.Rebus
{
    public class MsmqBus : BaseBus
    {
		private readonly string _endpoint;
        private Action<LoggingConfigurer> _loggingConfigurer;

        public MsmqBus(string endpoint)
        {
	        if (endpoint == null) throw new ArgumentNullException("endpoint");
	        Endpoint = endpoint;
	        _endpoint = endpoint;
	        Container = new BuiltinContainerAdapter();
            _loggingConfigurer = configurer => configurer.None();
        }

	    public override void RegisterHandler(Func<IHandleMessages> messageHandler)
	    {
			Container.Register(messageHandler);
	    }

	    public override void Send<T>(T message)
        {
            Guard.AgainstNullArgumentIfNullable("message", message);

	        var isAScript = message.GetType() == typeof(DefaultExecutionScript) || message.GetType().BaseType == typeof(DefaultExecutionScript);
	        if (SendBus == null)
	        {
				//Container.Handle<string>(ToConsole.WriteLine);
				//RegisterDefaultHandlers();

		        ConfigureSendBus(isAScript);
	        }

		    Guard.AgainstNullArgument("_sendBus", SendBus);

            Console.Write("Sending message of type {0}...", message.GetType().Name);
            try
            {
				SendBus.AttachHeader(message, "transport", "MSMQ");
                SendBus.Advanced.Routing.Send(_endpoint, message);
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
			//RegisterDefaultHandlers();

            return this;
        }

	    public override void Start()
        {
            if (ReceiveBus == null)
            {
                ConfigureReceiveBus();
            }

            Console.WriteLine("Awaiting messsage on {0}...", _endpoint);
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
				        configurer.UseMsmq(string.Format("{0}.reply", _endpoint),
					        string.Format("{0}.reply.error", _endpoint));
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
					    configurer.UseMsmq(_endpoint, string.Format("{0}.error", _endpoint)))
			    .CreateBus()
			    .Start();
	    }
    }
}