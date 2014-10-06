using System;
using Rebus.AzureServiceBus;
using Rebus.Configuration;
using Rebus.Logging;
using Rebus.Serialization.Json;

namespace ScriptCs.Rebus.AzureServiceBus
{
    public class AzureServiceBus : BaseBus
    {
        private readonly string _queue;
        private readonly string _azureConnectionString;
        private Action<LoggingConfigurer> _loggingConfigurer;
        private readonly BuiltinContainerAdapter _builtinContainerAdapter;

        public AzureServiceBus(string queue, string azureConnectionString)
        {
            Guard.AgainstNullArgument("queue", queue);
            Guard.AgainstNullArgument("azureConnectionString", azureConnectionString);

            _queue = queue;
            _azureConnectionString = azureConnectionString;
            _loggingConfigurer = configurer => configurer.None();

            _builtinContainerAdapter = new BuiltinContainerAdapter();
        }

        public override void SendAScript(string script)
        {
            Send(new Script {ScriptContent = script});
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
            SendBus.Advanced.Routing.Send(_queue, message);
            Console.WriteLine("... message sent.");

            ShutDown();
        }

        public override BaseBus Receive<T>(Action<T> action)
        {
            Guard.AgainstNullArgument("action", action);

            KnownTypes[typeof(T).Name] = typeof(T);
            _builtinContainerAdapter.Handle(action);

            return this;

        }

        public override void Start()
        {
            if (ReceiveBus == null)
            {
                ConfigureAzureReceiveBus();
            }

            Console.WriteLine("Awaiting messsage on {0}...", _queue);
        }

        public override BaseBus UseLogging()
        {
            _loggingConfigurer = configurer => configurer.Console();

            return this;
        }

        private void ConfigureAzureSendBus()
        {
            SendBus = Configure.With(new BuiltinContainerAdapter())
                .Logging(_loggingConfigurer)
                .Serialization(serializer => serializer.UseJsonSerializer()
                    .AddNameResolver(
                        x => x.Assembly.GetName().Name.Contains("ℛ")
                            ? new TypeDescriptor("ScriptCs.Compiled", x.Name)
                            : null))
                        .Transport(configurer => configurer.UseAzureServiceBus(_azureConnectionString, _queue, string.Format("{0}.error", _queue)))
                .CreateBus()
                .Start();
        }

        private void ConfigureAzureReceiveBus()
        {
            ReceiveBus = Configure.With(_builtinContainerAdapter)
                .Logging(_loggingConfigurer)
                .Serialization(serializer => serializer.UseJsonSerializer()
                    .AddTypeResolver(x => x.AssemblyName == "ScriptCs.Compiled" ? KnownTypes[x.TypeName] : null))
                .Transport(
                    configurer =>
                        configurer.UseAzureServiceBus(_azureConnectionString, _queue, string.Format("{0}.error", _queue)))
                .CreateBus()
                .Start();

        }
    }
}
