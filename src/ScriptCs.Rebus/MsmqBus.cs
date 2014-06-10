using System;
using Rebus;
using Rebus.Configuration;
using Rebus.Logging;
using Rebus.Serialization.Json;
using Rebus.Transports.Msmq;

namespace ScriptCs.Rebus
{
    public class MsmqBus : BaseBus
    {
        private readonly string _queue;
        private readonly BuiltinContainerAdapter _builtinContainerAdapter;
        private bool _useLogging = false;

        public MsmqBus(string queue)
        {
            Guard.AgainstNullArgument("queue", queue);

            _queue = queue;
            _builtinContainerAdapter = new BuiltinContainerAdapter();
        }

        public override void Send<T>(T message)
        {
            Guard.AgainstNullArgumentIfNullable("message", message);

            if (_sendBus == null)
            {
                ConfigureSendBus();
            }

            Guard.AgainstNullArgument("_sendBus", _sendBus);

            Console.WriteLine("Sending message of type {0}...", message.GetType().Name);
            _sendBus.Advanced.Routing.Send(_queue, message);
            Console.WriteLine("... message sent.");

            //ShutDown();
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
            if (_receiveBus == null)
            {
                ConfigureReceiveBus();
            }

            Console.WriteLine("Awaiting messsage on {0}...", _queue);
        }

        public override BaseBus UseLogging()
        {
            _useLogging = true;

            return this;
        }

        private void ConfigureSendBus()
        {
            var loggingConfigurer = _useLogging ? (configurer => configurer.Console()) :  new Action<LoggingConfigurer>(configurer => configurer.None());

            _sendBus = Configure.With(new BuiltinContainerAdapter())
                .Logging(loggingConfigurer)
                .Serialization(serializer => serializer.UseJsonSerializer()
                    .AddNameResolver(
                        x => x.Assembly.GetName().Name.Contains("ℛ")
                            ? new TypeDescriptor("ScriptCs.Compiled", x.Name)
                            : null))
                .Transport(configurer => configurer.UseMsmq(string.Format("{0}.input", _queue), string.Format("{0}.error", _queue)))
                .CreateBus()
                .Start();
        }

        private void ConfigureReceiveBus()
        {
            _receiveBus = Configure.With(_builtinContainerAdapter)
                .Logging(configurer => configurer.Console())
                .Serialization(serializer => serializer.UseJsonSerializer()
                    .AddTypeResolver(x => x.AssemblyName == "ScriptCs.Compiled" ? KnownTypes[x.TypeName] : null))
                .Transport(configurer => configurer.UseMsmq(_queue, string.Format("{0}.error", _queue)))
                .CreateBus()
                .Start();
        }


    }
}