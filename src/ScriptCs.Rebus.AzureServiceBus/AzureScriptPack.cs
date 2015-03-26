using ScriptCs.Contracts;

namespace ScriptCs.Rebus.AzureServiceBus
{
    public class AzureScriptPack : IScriptPack
    {
        public void Initialize(IScriptPackSession session)
        {
            Guard.AgainstNullArgument("session", session);

			// Rebus namespaces
            session.ImportNamespace("Rebus.AzureServiceBus");

			//ScriptCs namespaces
			session.ImportNamespace("ScriptCs.Contracts");

			// Local namespaces
			session.ImportNamespace("ScriptCs.Rebus.AzureServiceBus");
			session.ImportNamespace("ScriptCs.Rebus.Scripts");
			session.ImportNamespace("ScriptCs.Rebus.Configuration");
			session.ImportNamespace("ScriptCs.Rebus.Extensions");
			session.ImportNamespace("ScriptCs.Rebus.Logging");
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