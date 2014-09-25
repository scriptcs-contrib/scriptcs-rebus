using System;
using Rebus.Configuration;
using Rebus.Logging;

namespace ScriptCs.Rebus
{
    public class AzureBus : BaseBus
    {
        private Action<LoggingConfigurer> _loggingConfigurer;

        public AzureBus()
        {
            _loggingConfigurer = configurer => configurer.None();
        }

        public override void Send<T>(T message)
        {
            throw new NotImplementedException();
        }

        public override BaseBus Receive<T>(Action<T> action)
        {
            throw new NotImplementedException();
        }

        public override void Start()
        {
            throw new NotImplementedException();
        }

        public override BaseBus UseLogging()
        {
            _loggingConfigurer = configurer => configurer.Console();

            return this;
        }
    }
}