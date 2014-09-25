namespace ScriptCs.Rebus.AzureServiceBus
{
    public static class RebusScriptBusExtensions
    {
        public static BaseBus ConfigureAzureBus(this RebusScriptBus bus, string queue, string connectionString)
        {
            Guard.AgainstNullArgument("queue", queue);
            Guard.AgainstNullArgument("connectionString", connectionString);

            return new AzureServiceBus(queue, connectionString);
        }

    }
}