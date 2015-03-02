using System;
using Rebus;
using ScriptCs.Rebus.Scripts;

namespace ScriptCs.Rebus.Hosting.ScriptHandlers
{
	public class DefaultScriptExecutionHandler : IHandleMessages<DefaultExecutionScript>
	{
		private readonly IBus _bus;

		public DefaultScriptExecutionHandler(IBus bus)
		{
			if (bus == null) throw new ArgumentNullException("bus");
			_bus = bus;
		}

		public void Handle(DefaultExecutionScript message)
		{
			ScriptExecutor.Init(message);

			var scriptResult = ScriptExecutor.ExecuteScript();
			if (scriptResult.CompileExceptionInfo != null)
			{
				if (scriptResult.CompileExceptionInfo.SourceException != null)
				{
					_bus.Reply(scriptResult.CompileExceptionInfo.SourceException.Message);
				}
			}
			else
			{
				_bus.Reply("Executed with no errors!");
			}
		}
	}
}