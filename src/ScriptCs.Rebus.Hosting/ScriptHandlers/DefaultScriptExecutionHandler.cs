using Rebus;
using ScriptCs.Rebus.Scripts;

namespace ScriptCs.Rebus.Hosting.ScriptHandlers
{
	public class DefaultScriptExecutionHandler : IHandleMessages<DefaultExecutionScript>
    {
        public void Handle(DefaultExecutionScript message)
        {
	        var executor = new ScriptExecutor(message);
			executor.Execute();
        }
    }
}