using ScriptCs.Contracts;

namespace ScriptCs.Rebus
{
    public class RebusScriptPack : IScriptPack
    {
        private IScriptPackSession _session;

        public void Initialize(IScriptPackSession session)
        {
            Guard.AgainstNullArgument("session", session);

            session.ImportNamespace("Rebus");
            session.ImportNamespace("Rebus.Bus");
            session.ImportNamespace("Rebus.Logging");
            session.ImportNamespace("Rebus.Configuration");
            session.ImportNamespace("Rebus.Transports.Msmq");

            _session = session;
        }

        public IScriptPackContext GetContext()
        {
            return new RebusScriptBus(_session);
        }

        public void Terminate()
        {}
    }
}
