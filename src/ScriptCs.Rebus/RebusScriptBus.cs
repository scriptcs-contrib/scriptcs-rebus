using ScriptCs.Contracts;

namespace ScriptCs.Rebus
{
    public class RebusScriptBus : IScriptPackContext
    {
        public IScriptPackSession ScriptPackSession { get; private set; }

        public RebusScriptBus(IScriptPackSession session)
        {
            ScriptPackSession = session;
        }

        public BaseBus ConfigureBus(string endpoint)
        {
            return new MsmqBus(endpoint);
        }
    }

}
