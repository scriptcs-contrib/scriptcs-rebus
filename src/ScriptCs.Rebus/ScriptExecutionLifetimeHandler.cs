using System;
using Rebus;

namespace ScriptCs.Rebus
{
	public class ScriptExecutionLifetimeHandler : IHandleMessages<Int64>
	{
		public void Handle(Int64 message)
		{
			switch (message)
			{
				case 0:
					Console.WriteLine("Entering messaging console...");
					break;
				default:
					Console.WriteLine("Terminating messaging console...");
					break;
			}
		}
	}
}