namespace ScriptCs.Rebus.RabbitMQ
{
    public static class RebusScriptBusExtensions
    {
        private const string RabbitMqDefaultConnectionString = "amqp://localhost:5672";

        public static BaseBus ConfigureRabbitBus(this RebusScriptBus bus, string endpoint, string connectionString = RabbitMqDefaultConnectionString)
        {
            Guard.AgainstNullArgument("endpoint", endpoint);
            Guard.AgainstNullArgument("connectionString", connectionString);

            bus.ScriptPackSession.ImportNamespace("Rebus.RabbitMQ");
            
            return new RabbitMqBus(endpoint, connectionString);
        }
    }
}