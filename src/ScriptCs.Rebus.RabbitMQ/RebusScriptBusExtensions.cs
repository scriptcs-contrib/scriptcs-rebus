namespace ScriptCs.Rebus
{
    public static class RebusScriptBusExtensions
    {
        private const string RabbitMqDefaultConnectionString = "amqp://localhost:5672";

        public static BaseBus ConfigureRabbitBus(this RebusScriptBus bus, string queue, string connectionString = RabbitMqDefaultConnectionString)
        {
            Guard.AgainstNullArgument("queue", queue);
            Guard.AgainstNullArgument("connectionString", connectionString);

            bus.ScriptPackSession.ImportNamespace("Rebus.RabbitMQ");
            
            return new RabbitMqBus(queue, connectionString);
        }
    }
}