using System.IO;
using System.Runtime.CompilerServices;
using ScriptCs.Rebus.Configuration;
using ScriptCs.Rebus.Scripts;

namespace ScriptCs.Rebus.Extensions
{
	public static class ScriptConfigurationExtensions
	{
		public static ScriptConfiguration AScript(
			this ScriptConfiguration scriptConfiguration, string script)
		{
			scriptConfiguration.ScriptContent = script;
			return scriptConfiguration;

			//return scriptConfiguration.Configuration(new DefaultExecutionScript
			//{
			//	ScriptContent = script,
			//	NuGetDependencies = scriptConfiguration.NugetDependencies.ToArray(),

			//	Namespaces = scriptConfiguration.Namespaces.ToArray(),
			//	LocalDependencies = scriptConfiguration.LocalDependencies.ToArray(),
			//	UseMono = scriptConfiguration.UseMonoVar,

			//	// Internalize method
			//	LogLevel = scriptConfiguration.GetLogLevel()
			//});
		}

		public static ScriptConfiguration AScriptFile(this ScriptConfiguration scriptConfiguration,
			string scriptFile)
		{
			scriptConfiguration.ScriptContent = File.ReadAllText(scriptFile);
			return scriptConfiguration;

			//return scriptConfiguration.Configuration(new DefaultExecutionScript
			//{
			//	ScriptContent = File.ReadAllText(scriptFile),
			//	NuGetDependencies = scriptConfiguration.NugetDependencies.ToArray(),

			//	Namespaces = scriptConfiguration.Namespaces.ToArray(),
			//	LocalDependencies = scriptConfiguration.LocalDependencies.ToArray(),
			//	UseMono = scriptConfiguration.UseMonoVar,
			//	LogLevel = scriptConfiguration.GetLogLevel()
			//});
		}
	}
}