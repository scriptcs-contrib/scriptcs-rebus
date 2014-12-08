﻿using System;
using Rebus;
using Rebus.Configuration;
using Rebus.Logging;
using Rebus.Serialization.Json;
using Rebus.Transports.Msmq;

namespace ScriptCs.Rebus
{
    public class MsmqBus : BaseBus
    {
        private readonly string _endpoint;
        private Action<LoggingConfigurer> _loggingConfigurer;

        public MsmqBus(string endpoint)
        {
            _endpoint = endpoint;
            Container = new BuiltinContainerAdapter();
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
                ConfigureReceiveBus();
            }

            Console.WriteLine("Awaiting messsage on {0}...", _endpoint);
        }

        public override BaseBus UseLogging()
        {
            _loggingConfigurer = configurer => configurer.Console();

            return this;
        }

        private void ConfigureSendBus()
        {
            SendBus = Configure.With(Container)
                .Logging(_loggingConfigurer)
                .Serialization(serializer => serializer.UseJsonSerializer()
                    .AddNameResolver(
                        x => x.Assembly.GetName().Name.Contains("ℛ")
                            ? new TypeDescriptor("ScriptCs.Compiled", x.Name)
                            : null))
                .Transport(configurer => configurer.UseMsmqInOneWayClientMode())
                .CreateBus()
                .Start();
        }

        private void ConfigureReceiveBus()
        {
            ReceiveBus = Configure.With(Container)
                .Logging(_loggingConfigurer)
                .Serialization(serializer => serializer.UseJsonSerializer()
                    .AddTypeResolver(x => x.AssemblyName == "ScriptCs.Compiled" ? KnownTypes[x.TypeName] : null))
                .Transport(configurer => configurer.UseMsmq(_endpoint, string.Format("{0}.error", _endpoint)))
                .CreateBus()
                .Start();
        }
    }
}