using ScriptCs.Contracts;

namespace ScriptCs.Rebus.RabbitMQ
{
    public class RabbitScriptPack : IScriptPack
    {
        public void Initialize(IScriptPackSession session)
        {
            Guard.AgainstNullArgument("session", session);

            session.ImportNamespace("Rebus.RabbitMQ");
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