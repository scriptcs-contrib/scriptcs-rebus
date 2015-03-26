using System;
using Rebus;
using Rebus.Configuration;
using Rebus.Logging;
using Rebus.RabbitMQ;
using Rebus.Serialization.Json;
using ScriptCs.Rebus.Configuration;

namespace ScriptCs.Rebus.RabbitMQ
{
    public class RabbitMqBus : BaseBus
    {
        private readonly string _rabbitConnectionString;
        private Action<LoggingConfigurer> _loggingConfigurer;


        public RabbitMqBus(string endpoint, string connectionString)
        {
            Guard.AgainstNullArgument("connectionString", connectionString);

	        Endpoint = endpoint;
            _rabbitConnectionString = connectionString;
            Container = new BuiltinContainerAdapter();
            _loggingConfigurer = configurer => configurer.None();
        }

	    public override void Send<T>(T message)
        {
            Guard.AgainstNullArgumentIfNullable("message", message);

			var isAScript = message.GetType() == typeof(IExecutionScript) || message.GetType().BaseType == typeof(IExecutionScript);
			if (SendBus == null)
            {
                ConfigureRabbitSendBus(isAScript);
            }

            Guard.AgainstNullArgument("_sendBus", SendBus);

			// Add header information
			if (isAScript)
			{
				SendBus.AttachHeader(message, "connectionString", _rabbitConnectionString);
				SendBus.AttachHeader(message, "transport", "RABBIT");
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
                ConfigureRabbitReceiveBus();
            }

            Console.WriteLine("Awaiting messsage on {0}...", Endpoint);
        }

        public override BaseBus UseLogging()
        {
            _loggingConfigurer = configurer => configurer.Console();

            return this;
        }

        private void ConfigureRabbitSendBus(bool isAScript)
        {
	        Action<RebusTransportConfigurer> transportConfig;
	        if (!isAScript)
	        {
		        transportConfig =
			        configurer =>
				        configurer.UseRabbitMqInOneWayMode(_rabbitConnectionString);
	        }
	        else
	        {
		        transportConfig =
			        configurer =>
				        configurer.UseRabbitMq(_rabbitConnectionString, string.Format("{0}.reply", Endpoint),
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

        private void ConfigureRabbitReceiveBus()
        {
            ReceiveBus = Configure.With(Container)
                .Logging(_loggingConfigurer)
                .Serialization(serializer => serializer.UseJsonSerializer()
                    .AddTypeResolver(x => x.AssemblyName == "ScriptCs.Compiled" ? KnownTypes[x.TypeName] : null))
                .Transport(configurer => configurer.UseRabbitMq(_rabbitConnectionString, Endpoint, string.Format("{0}.error", Endpoint)))
                .CreateBus()
                .Start();
        }


    }
}
