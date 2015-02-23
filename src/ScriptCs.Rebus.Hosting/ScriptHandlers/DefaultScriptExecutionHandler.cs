using Rebus;
using ScriptCs.Rebus.Scripts;

namespace ScriptCs.Rebus.Hosting.ScriptHandlers
{
	public class DefaultScriptExecutionHandler : IHandleMessages<DefaultExecutionScript>
    {
        public void Handle(DefaultExecutionScript message)
        {
			ScriptExecutor.Init(message);

			ScriptExecutor.ExecuteScript();
        }
    }
}