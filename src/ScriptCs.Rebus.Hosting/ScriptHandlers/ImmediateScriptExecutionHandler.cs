using Rebus;
using ScriptCs.Rebus.Scripts;

namespace ScriptCs.Rebus.Hosting.ScriptHandlers
{
    public class ImmediateScriptExecutionHandler : IHandleMessages<ImmediateExecutionScript>
    {
        public void Handle(ImmediateExecutionScript message)
        {
            var executor = new ScriptExecutor();
            executor.Execute(message);
        }
    }
}