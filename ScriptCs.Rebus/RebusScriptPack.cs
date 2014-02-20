using ScriptCs.Contracts;

namespace ScriptCs.Rebus
{
    public class RebusScriptPack : IScriptPack
    {
        public void Initialize(IScriptPackSession session)
        {
            throw new System.NotImplementedException();
        }

        public IScriptPackContext GetContext()
        {
            throw new System.NotImplementedException();
        }

        public void Terminate()
        {}
    }
}
