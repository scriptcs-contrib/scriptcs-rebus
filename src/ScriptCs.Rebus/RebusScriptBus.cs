using ScriptCs.Contracts;

namespace ScriptCs.Rebus
{
    public class RebusScriptBus : IScriptPackContext
    {
        public BaseBus ConfigureBus(string endpoint)
        {
            return new MsmqBus(endpoint);
        }
    }

}
