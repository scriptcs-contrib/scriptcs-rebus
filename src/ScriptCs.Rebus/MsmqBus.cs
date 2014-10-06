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
        private Action<LoggingConfigurer> _loggingConfigurer;
        private BuiltinContainerAdapter _builtinContainerAdapter;

        public MsmqBus(string queue)
        {
            Guard.AgainstNullArgument("queue", queue);

            _queue = queue;
            _builtinContainerAdapter = new BuiltinContainerAdapter();
            _loggingConfigurer = configurer => configurer.None();
        }

        public override void Send<T>(T message)
        {
            Guard.AgainstNullArgumentIfNullable("message", message);

            if (SendBus == null)
            {
                ConfigureSendBus();
            }

            Guard.AgainstNullArgument("_sendBus", SendBus);

            Console.WriteLine("Sending message of type {0}...", message.GetType().Name);
            try
            {
                SendBus.Advanced.Routing.Send(_queue, message);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            finally
            {
                ShutDown();
            }

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
                ConfigureReceiveBus();
            }

            Console.WriteLine("Awaiting messsage on {0}...", _queue);
        }

        public override BaseBus UseLogging()
        {
            _loggingConfigurer = configurer => configurer.Console();

            return this;
        }

        private void ConfigureSendBus()
        {
            SendBus = Configure.With(new BuiltinContainerAdapter())
                .Logging(_loggingConfigurer)
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
            ReceiveBus = Configure.With(_builtinContainerAdapter)
                .Logging(_loggingConfigurer)
                .Serialization(serializer => serializer.UseJsonSerializer()
                    .AddTypeResolver(x => x.AssemblyName == "ScriptCs.Compiled" ? KnownTypes[x.TypeName] : null))
                .Transport(configurer => configurer.UseMsmq(_queue, string.Format("{0}.error", _queue)))
                .CreateBus()
                .Start();
        }


    }
}