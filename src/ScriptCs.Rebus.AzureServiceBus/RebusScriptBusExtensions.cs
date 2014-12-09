namespace ScriptCs.Rebus.AzureServiceBus
{
    public static class RebusScriptBusExtensions
    {
        public static BaseBus ConfigureAzureBus(this RebusScriptBus bus, string endpoint, string connectionString)
        {
            Guard.AgainstNullArgument("endpoint", endpoint);
            Guard.AgainstNullArgument("connectionString", connectionString);

            return new AzureServiceBus(endpoint, connectionString);
        }

    }
}