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
			return
				scriptConfiguration.Configuration(
					() => scriptConfiguration.Bus.Send(new DefaultExecutionScript
					{
						ScriptContent = script,
						NuGetDependencies = scriptConfiguration.NugetDependencies.ToArray(),

						Namespaces = scriptConfiguration.Namespaces.ToArray(),
						LocalDependencies = scriptConfiguration.LocalDependencies.ToArray(),
						UseMono = scriptConfiguration.UseMonoVar,
						LogLevel = scriptConfiguration.GetLogLevel()
					}));
		}

		public static ScriptConfiguration AScriptFile(this ScriptConfiguration scriptConfiguration,
			string scriptFile)
		{
			return
				scriptConfiguration.Configuration(
					() => scriptConfiguration.Bus.Send(new DefaultExecutionScript
					{
						ScriptContent = File.ReadAllText(scriptFile),
						NuGetDependencies = scriptConfiguration.NugetDependencies.ToArray(),

						Namespaces = scriptConfiguration.Namespaces.ToArray(),
						LocalDependencies = scriptConfiguration.LocalDependencies.ToArray(),
						UseMono = scriptConfiguration.UseMonoVar,
						LogLevel = scriptConfiguration.GetLogLevel()
					}));
		}
	}
}