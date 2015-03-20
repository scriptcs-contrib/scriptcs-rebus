using ScriptCs.Contracts;

namespace ScriptCs.Rebus
{
    public class RebusScriptPack : IScriptPack
    {
        public void Initialize(IScriptPackSession session)
        {
            Guard.AgainstNullArgument("session", session);

			// Rebus namespaces
            session.ImportNamespace("Rebus");
            session.ImportNamespace("Rebus.Bus");
            //session.ImportNamespace("Rebus.Logging");
            session.ImportNamespace("Rebus.Configuration");
            session.ImportNamespace("Rebus.Transports.Msmq");

			//ScriptCs namespaces
			
			session.ImportNamespace("ScriptCs.Contracts");

			// Local namespaces
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
        {}
    }
}
