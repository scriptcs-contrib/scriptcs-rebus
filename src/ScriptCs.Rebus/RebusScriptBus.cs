using System;
using Rebus;
using Rebus.Bus;
using Rebus.Configuration;
using Rebus.Logging;
using Rebus.Transports.Msmq;
using ScriptCs.Contracts;

namespace ScriptCs.Rebus
{
    public class RebusScriptBus : IScriptPackContext
    {
        private IBus _bus;

        public void Send(string destination, object message)
        {
            Guard.AgainstNullArgument("destination", destination);
            Guard.AgainstNullArgumentIfNullable("message", message);

            ConfigureRebus();

            Guard.AgainstNullArgument("_bus", _bus);

            Console.WriteLine("Sending message of type {0}...", message.GetType().Name);
            _bus.Advanced.Routing.Send(destination, message);
            Console.WriteLine("... message sent.");

            ShutDown();
        }

        private void ConfigureRebus()
        {
            _bus = Configure.With(new BuiltinContainerAdapter())
                .Logging(configurer => configurer.None())
                .Transport(configurer => configurer.UseMsmq("ScriptCs.Rebus.Input", "ScriptCs.Rebus.Error"))
                .CreateBus()
                .Start();
        }

        private void ShutDown()
        {
            _bus.Dispose();
        }
    }
}