﻿using System;
using System.Diagnostics;
using System.Threading;
using Rebus.Configuration;
using Rebus.Logging;
using Rebus.RabbitMQ;
using Rebus.Serialization.Json;

namespace ScriptCs.Rebus
{
    public class RabbitMqBus : BaseBus
    {
        private readonly string _queue;
        private readonly string _rabbitConnectionString;
        private Action<LoggingConfigurer> _loggingConfigurer;


        public RabbitMqBus(string queue, string connectionString)
        {
            Guard.AgainstNullArgument("queue", queue);
            Guard.AgainstNullArgument("connectionString", connectionString);
            
            _queue = queue;
            _rabbitConnectionString = connectionString;
            Container = new BuiltinContainerAdapter();
            _loggingConfigurer = configurer => configurer.None();
        }

        public override void Send<T>(T message)
        {
            Guard.AgainstNullArgumentIfNullable("message", message);

            if (SendBus == null)
            {
                ConfigureRabbitSendBus();
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
            Container.Handle(action);

            return this;
        }

        public override void Start()
        {
            if (ReceiveBus == null)
            {
                ConfigureRabbitReceiveBus();
            }

            Console.WriteLine("Awaiting messsage on {0}...", _queue);
        }

        public override BaseBus UseLogging()
        {
            _loggingConfigurer = configurer => configurer.Console();

            return this;
        }

        private void ConfigureRabbitSendBus()
        {
            SendBus = Configure.With(Container)
                .Logging(_loggingConfigurer)
                .Serialization(serializer => serializer.UseJsonSerializer()
                    .AddNameResolver(
                        x => x.Assembly.GetName().Name.Contains("ℛ")
                            ? new TypeDescriptor("ScriptCs.Compiled", x.Name)
                            : null))
                        .Transport(configurer => configurer.UseRabbitMq(_rabbitConnectionString, _queue, string.Format("{0}.error", _queue)))
                .CreateBus()
                .Start();
        }

        private void ConfigureRabbitReceiveBus()
        {
            ReceiveBus = Configure.With(Container)
                .Logging(_loggingConfigurer)
                .Serialization(serializer => serializer.UseJsonSerializer()
                    .AddTypeResolver(x => x.AssemblyName == "ScriptCs.Compiled" ? KnownTypes[x.TypeName] : null))
                .Transport(configurer => configurer.UseRabbitMq(_rabbitConnectionString, _queue, string.Format("{0}.error", _queue)))
                .CreateBus()
                .Start();
        }


    }
}
