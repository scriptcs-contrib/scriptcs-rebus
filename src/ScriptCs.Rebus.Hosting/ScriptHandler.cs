using Rebus;

namespace ScriptCs.Rebus.Hosting
{
    public class ScriptHandler : IHandleMessages<Script>
    {
        public void Handle(Script message)
        {
            var executor = new ScriptExecutor();
            executor.Execute(message);
        }
    }
}