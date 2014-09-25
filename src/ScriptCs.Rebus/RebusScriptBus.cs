using ScriptCs.Contracts;

namespace ScriptCs.Rebus
{
    public class RebusScriptBus : IScriptPackContext
    {
        public BaseBus ConfigureBus(string queue)
        {
            Guard.AgainstNullArgument("queue", queue);

            return new MsmqBus(queue);
        }
    }

}
