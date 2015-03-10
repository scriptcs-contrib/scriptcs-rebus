using ScriptCs.Contracts;

namespace ScriptCs.Rebus
{
    public class RebusScriptPack : IScriptPack
    {
        public void Initialize(IScriptPackSession session)
        {
            Guard.AgainstNullArgument("session", session);

            session.ImportNamespace("Rebus");
            session.ImportNamespace("Rebus.Bus");
            session.ImportNamespace("Rebus.Logging");
            session.ImportNamespace("Rebus.Configuration");
            session.ImportNamespace("Rebus.Transports.Msmq");
			session.ImportNamespace("ScriptCs.Rebus.Scripts");
        }

        public IScriptPackContext GetContext()
        {
            return new RebusScriptBus();
        }

        public void Terminate()
        {}
    }
}
