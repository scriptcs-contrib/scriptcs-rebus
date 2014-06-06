using System;
using System.Collections.Concurrent;
using System.Messaging;
using Rebus;
using Rebus.Configuration;
using Rebus.Logging;
using Rebus.RabbitMQ;
using Rebus.Serialization.Json;
using Rebus.Transports.Msmq;
using ScriptCs.Contracts;

namespace ScriptCs.Rebus
{
    public class RebusScriptBus : IScriptPackContext
    {
        private const string RabbitMqDefaultConnectionString = "amqp://localhost:5672";
        private string _queue;
        private IBus _sendBus;
        private IBus _receiveBus;
        private readonly BuiltinContainerAdapter _builtinContainerAdapter;
        private bool _isRabbitMq;
        private string _rabbitConnectionString;

        public RebusScriptBus()
        {
            _builtinContainerAdapter = new BuiltinContainerAdapter();
        }

        public RebusScriptBus ConfigureBus(string queue)
        {
            Guard.AgainstNullArgument("queue", queue);

            _queue = queue;

            return this;
        }

        public RebusScriptBus ConfigureRabbitBus(string queue, string connectionString = RabbitMqDefaultConnectionString)
        {
            Guard.AgainstNullArgument("queue", queue);
            Guard.AgainstNullArgument("connectionString", connectionString);

            _queue = queue;
            _rabbitConnectionString = connectionString;
            _isRabbitMq = true;

            return this;
        }

        public void Send<T>(T message) where T : class
        {
            Guard.AgainstNullArgumentIfNullable("message", message);

            if (_sendBus == null)
            {
                if (_isRabbitMq)
                {
                    ConfigureRabbitSendBus();
                }
                else
                {
                    ConfigureSendBus();
                }
            }

            Guard.AgainstNullArgument("_sendBus", _sendBus);

            Console.WriteLine("Sending message of type {0}...", message.GetType().Name);
            _sendBus.Advanced.Routing.Send(_queue, message);
            Console.WriteLine("... message sent.");

            ShutDown();
        }

        public RebusScriptBus Receive<T>(Action<T> action) where T : class
        {
            Guard.AgainstNullArgument("action", action);

            knownTypes[typeof(T).Name] = typeof(T);
            _builtinContainerAdapter.Register(() => new MessageReceiver<T>(action));

            return this;
        }

        public void Start()
        {
            if (_receiveBus == null)
            {
                if (_isRabbitMq)
                {
                    ConfigureRabbitReceiveBus();
                }
                else
                {
                    ConfigureReceiveBus();
                }
            }

            Console.WriteLine("Awaiting messsage on {0}...", _queue);
        }

        private void ConfigureRabbitSendBus()
        {
            _sendBus = Configure.With(new BuiltinContainerAdapter())
                .Logging(configurer => configurer.None())
                .Serialization(serializer => serializer.UseJsonSerializer()
                    .AddNameResolver(
                        x => x.Assembly.GetName().Name.Contains("ℛ")
                            ? new TypeDescriptor("ScriptCs.Compiled", x.Name)
                            : null))
                        .Transport(configurer => configurer.UseRabbitMq(_rabbitConnectionString, _queue, string.Format("{0}.error", _queue)))
                .CreateBus()
                .Start();
        }

        private void ConfigureSendBus()
        {
            _sendBus = Configure.With(new BuiltinContainerAdapter())
                .Logging(configurer => configurer.None())
                .Serialization(serializer => serializer.UseJsonSerializer()
                    .AddNameResolver(
                        x => x.Assembly.GetName().Name.Contains("ℛ")
                            ? new TypeDescriptor("ScriptCs.Compiled", x.Name)
                            : null))
                        .Transport(configurer => configurer.UseMsmq(_queue, string.Format("{0}.error", _queue)))
                .CreateBus()
                .Start();
        }

        private void ShutDown()
        {
            if (_sendBus != null) _sendBus.Dispose();
            if (_receiveBus != null) _receiveBus.Dispose();
        }

        readonly ConcurrentDictionary<string, Type> knownTypes = new ConcurrentDictionary<string, Type>();

        private void ConfigureRabbitReceiveBus()
        {
            _receiveBus = Configure.With(_builtinContainerAdapter)
                .Logging(configurer => configurer.None())
                .Serialization(serializer => serializer.UseJsonSerializer()
                    .AddTypeResolver(x => x.AssemblyName == "ScriptCs.Compiled" ? knownTypes[x.TypeName] : null))
                .Transport(configurer => configurer.UseRabbitMq(_rabbitConnectionString, _queue, string.Format("{0}.error", _queue)))
                .CreateBus()
                .Start();
        }

        private void ConfigureReceiveBus()
        {
            _receiveBus = Configure.With(_builtinContainerAdapter)
                .Logging(configurer => configurer.None())
                .Serialization(serializer => serializer.UseJsonSerializer()
                    .AddTypeResolver(x => x.AssemblyName == "ScriptCs.Compiled" ? knownTypes[x.TypeName] : null))
                .Transport(configurer => configurer.UseMsmq(_queue, string.Format("{0}.error", _queue)))
                .CreateBus()
                .Start();
        }
    }

    internal class MessageReceiver<T> : IHandleMessages<T> where T : class
    {
        private readonly Action<T> _action;

        public MessageReceiver(Action<T> action)
        {
            Guard.AgainstNullArgument("action", action);
            
            _action = action;
        }

        public void Handle(T message)
        {
            Guard.AgainstNullArgument("message", message);

            _action(message);
        }
    }

}
