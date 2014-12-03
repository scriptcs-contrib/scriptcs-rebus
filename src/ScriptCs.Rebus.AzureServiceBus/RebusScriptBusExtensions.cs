namespace ScriptCs.Rebus
{
    public static class RebusScriptBusExtensions
    {
        public static BaseBus ConfigureAzureBus(this RebusScriptBus bus, string queue, string connectionString)
        {
            Guard.AgainstNullArgument("queue", queue);
            Guard.AgainstNullArgument("connectionString", connectionString);

            bus.ScriptPackSession.ImportNamespace("Rebus.AzureServiceBus");

            return new AzureServiceBus(queue, connectionString);
        }

    }
}