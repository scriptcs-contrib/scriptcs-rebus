using ScriptCs.Contracts;

namespace ScriptCs.Rebus
{
    public class RebusScriptBus : IScriptPackContext
    {
        private const string RabbitMqDefaultConnectionString = "amqp://localhost:5672";

        public MsmqBus ConfigureBus(string queue)
        {
            Guard.AgainstNullArgument("queue", queue);

            return new MsmqBus(queue);
        }

        public RabbitMqBus ConfigureRabbitBus(string queue, string connectionString = RabbitMqDefaultConnectionString)
        {
            Guard.AgainstNullArgument("queue", queue);
            Guard.AgainstNullArgument("connectionString", connectionString);

            return new RabbitMqBus(queue, connectionString);
        }
    }

}
