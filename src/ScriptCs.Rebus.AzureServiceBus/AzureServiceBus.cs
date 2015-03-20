using System;
using Rebus;
using Rebus.AzureServiceBus;
using Rebus.Configuration;
using Rebus.Logging;
using Rebus.Serialization.Json;

namespace ScriptCs.Rebus.AzureServiceBus
{
    public class AzureServiceBus : BaseBus
    {
        private readonly string _endpoint;
        private readonly string _azureConnectionString;
        private Action<LoggingConfigurer> _loggingConfigurer;

        public AzureServiceBus(string endpoint, string azureConnectionString)
        {
            Guard.AgainstNullArgument("endpoint", endpoint);
            Guard.AgainstNullArgument("azureConnectionString", azureConnectionString);

            _endpoint = endpoint;
            _azureConnectionString = azureConnectionString;
            _loggingConfigurer = configurer => configurer.None();

            Container = new BuiltinContainerAdapter();
        }

	    public override void RegisterHandler(Func<IHandleMessages> messageHandler)
	    {
		    throw new NotImplementedException();
	    }

	    public override void Send<T>(T message)
        {
            Guard.AgainstNullArgument("message", message);

            if (SendBus == null)
            {
                ConfigureAzureSendBus();
            }

            Guard.AgainstNullArgument("_sendBus", SendBus);

            Console.WriteLine("Sending message of type {0}...", message.GetType().Name);
            SendBus.Advanced.Routing.Send(_endpoint, message);
            Console.WriteLine("... message sent.");

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
                ConfigureAzureReceiveBus();
            }

            Console.WriteLine("Awaiting messsage on {0}...", _endpoint);
        }

        public override BaseBus UseLogging()
        {
            _loggingConfigurer = configurer => configurer.Console();

            return this;
        }

        private void ConfigureAzureSendBus()
        {
            SendBus = Configure.With(Container)
                .Logging(_loggingConfigurer)
                .Serialization(serializer => serializer.UseJsonSerializer()
                    .AddNameResolver(
                        x => x.Assembly.GetName().Name.Contains("ℛ")
                            ? new TypeDescriptor("ScriptCs.Compiled", x.Name)
                            : null))
                        .Transport(configurer => configurer.UseAzureServiceBusInOneWayClientMode(_azureConnectionString))
                .CreateBus()
                .Start();
        }

        private void ConfigureAzureReceiveBus()
        {
            ReceiveBus = Configure.With(Container)
                .Logging(_loggingConfigurer)
                .Serialization(serializer => serializer.UseJsonSerializer()
                    .AddTypeResolver(x => x.AssemblyName == "ScriptCs.Compiled" ? KnownTypes[x.TypeName] : null))
                .Transport(
                    configurer =>
                        configurer.UseAzureServiceBus(_azureConnectionString, _endpoint, string.Format("{0}.error", _endpoint)))
                .CreateBus()
                .Start();

        }
    }
}
