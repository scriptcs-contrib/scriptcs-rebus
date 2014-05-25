using System;
using System.Collections.Concurrent;
using System.Messaging;
using Rebus;
using Rebus.Configuration;
using Rebus.Logging;
using Rebus.Serialization.Json;
using Rebus.Transports.Msmq;
using ScriptCs.Contracts;

namespace ScriptCs.Rebus
{
    public class RebusScriptBus : IScriptPackContext
    {
        private string _queue;
        private IBus _sendBus;
        private IBus _receiveBus;
        private readonly BuiltinContainerAdapter _builtinContainerAdapter;

        public RebusScriptBus()
        {
            _builtinContainerAdapter = new BuiltinContainerAdapter();
        }

        public RebusScriptBus ConfigureBus(string queue)
        {
            if (queue == null) throw new ArgumentNullException("queue");

            _queue = queue;

            return this;
        }

        public void Send<T>(T message) where T : class
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

            ShutDown();
        }

        public RebusScriptBus Receive<T>(Action<T> f) where T : class
        {
            knownTypes[typeof(T).Name] = typeof(T);
            _builtinContainerAdapter.Register(() => new MessageReceiver<T>(_queue, f));


            return this;
        }

        public void Start()
        {
            if (_receiveBus == null)
            {
                ConfigureReceiveBus();
            }
        }

        private void ConfigureSendBus()
        {
            CreateQueue(_queue);

            _sendBus = Configure.With(_builtinContainerAdapter)
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

        private void CreateQueue(string queueName)
        {
            var path = string.Format(@".\private$\{0}", queueName);

            if (!MessageQueue.Exists(path))
            {
                MessageQueue.Create(path, true);
            }
        }

        private void ShutDown()
        {
            if (_sendBus != null) _sendBus.Dispose();
            if (_receiveBus != null) _receiveBus.Dispose();
        }

        readonly ConcurrentDictionary<string, Type> knownTypes = new ConcurrentDictionary<string, Type>();
        
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
        private readonly string _queue;
        private readonly Action<T> _f;

        public MessageReceiver(string queue, Action<T> f)
        {
            if (queue == null) throw new ArgumentNullException("queue");
            _queue = queue;
            _f = f;
        }

        public void Handle(T message)
        {
            Console.Write("From {0} > ", _queue);
            _f(message);
        }
    }

}