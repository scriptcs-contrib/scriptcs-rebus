using ScriptCs.Contracts;

namespace ScriptCs.Rebus.Configuration
{
	public interface IExecutionScript
	{
		string ScriptContent { get; set; }
		bool UseMono { get; set; }
		string[] NuGetDependencies { get; set; }
		string[] Namespaces { get; set; }
		string[] LocalDependencies { get; set; }
		LogLevel LogLevel { get; set; }
	}
}