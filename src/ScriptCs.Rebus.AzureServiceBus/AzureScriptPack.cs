using ScriptCs.Contracts;

namespace ScriptCs.Rebus.AzureServiceBus
{
    public class AzureScriptPack : IScriptPack
    {
        public void Initialize(IScriptPackSession session)
        {
            Guard.AgainstNullArgument("session", session);

            session.ImportNamespace("Rebus.AzureServiceBus");
            session.ImportNamespace("ScriptCs.Rebus.AzureServiceBus");
        }

        public IScriptPackContext GetContext()
        {
            return new RebusScriptBus();
        }

        public void Terminate()
        {
            
        }
    }
}