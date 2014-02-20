using ScriptCs.Contracts;

namespace ScriptCs.Rebus
{
    public class RebusScriptPack : IScriptPack
    {
        public void Initialize(IScriptPackSession session)
        {
            session.AddReference("Rebus");
            session.ImportNamespace("Rebus");
        }

        public IScriptPackContext GetContext()
        {
            throw new System.NotImplementedException();
        }

        public void Terminate()
        {}
    }
}
