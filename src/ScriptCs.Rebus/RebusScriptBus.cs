using System;
using System.Collections.Concurrent;
using System.ComponentModel.Composition;
using System.Linq;
using System.Messaging;
using System.Reflection;
using System.Runtime.Remoting.Channels;
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
        private BuiltinContainerAdapter _builtinContainerAdapter;
        private TypeDescriptor _typeDescriptor; 

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
            Guard.AgainstNullArgument("destination", _queue);
            Guard.AgainstNullArgumentIfNullable("message", message);
                
            ConfigureSendBus<T>(_queue);

            Guard.AgainstNullArgument("_sendBus", _sendBus);

            Console.WriteLine("Sending message of type {0}...", message.GetType().Name);
            _sendBus.Advanced.Routing.Send(_queue, message);
            Console.WriteLine("... message sent.");

            ShutDown();
        }

        public RebusScriptBus Receive<T>(Action<T> f) where T : class
        {
            Console.WriteLine(typeof(T).Name);
            knownTypes[typeof (T).Name] = typeof(T);
            _builtinContainerAdapter.Register(() => new MessageReceiver<T>(f));

            Console.WriteLine("Awaiting messages of type {0}", typeof(T).Name);

            return this;
        }

        public void Start()
        {
            ConfigureReceiveBus();
        }

        private void ConfigureSendBus<T>(string destination) where T : class
        {
            Console.WriteLine(typeof(T).Assembly.FullName);

            _typeDescriptor = new TypeDescriptor("ScriptCs.Compiled", typeof(T).Name);

            CreateQueue(destination);
            _sendBus = Configure.With(_builtinContainerAdapter)
                .Logging(configurer => configurer.None())
                .Serialization(serializer => serializer.UseJsonSerializer()
                    //.AddTypeResolver(x => Assembly.GetExecutingAssembly().GetType("pingo" + "." + typeof(T).Name))
                    .AddNameResolver(x => new TypeDescriptor("ScriptCs.Compiled", x.Name)))
                .Transport(configurer => configurer.UseMsmq(destination, string.Format("{0}.error", destination)))
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
            _sendBus.Dispose();
        }
        
        readonly ConcurrentDictionary<string, Type> knownTypes = new ConcurrentDictionary<string, Type>();
        
        private void ConfigureReceiveBus()
        {

            var receivingBus = Configure.With(_builtinContainerAdapter)
                .Logging(configurer => configurer.None())
                .Serialization(serializer => serializer.UseJsonSerializer()
                    //.AddNameResolver(x => _typeDescriptor)
                    .AddTypeResolver(x =>
                    {
                        Console.WriteLine(x.TypeName);
                        var type = x.AssemblyName == "ScriptCs.Compiled" ? knownTypes[x.TypeName] : typeof (string);
                        return type;
                    }))
                .Transport(configurer => configurer.UseMsmq(_queue, string.Format("{0}.error", _queue)))
                .CreateBus()
                .Start();

            //receivingBus.Advanced.Routing.Subscribe<T>(inputQueue);
        }

        internal class MessageReceiver<T> : IHandleMessages<T> where T : class
        {
            private readonly Action<T> _f;

            public MessageReceiver(Action<T> f)
            {
                _f = f;
            }

            public void Handle(T message)
            {
                _f(message);
            }
        }
    }
}