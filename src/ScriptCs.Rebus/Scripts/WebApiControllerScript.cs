using System;
using ScriptCs.Contracts;
using ScriptCs.Rebus.Configuration;

namespace ScriptCs.Rebus.Scripts
{
	public class WebApiControllerScript : IExecutionScript
	{
		public string ControllerName { get; set; }

		public string ScriptContent { get; set; }

		public bool UseMono { get; set; }

		public string[] NuGetDependencies { get; set; }

		public string[] Namespaces { get; set; }

		public string[] LocalDependencies { get; set; }

		public LogLevel LogLevel { get; set; }
	}
}