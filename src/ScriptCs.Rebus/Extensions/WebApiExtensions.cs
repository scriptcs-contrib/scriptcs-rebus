using System.IO;
using ScriptCs.Rebus.Configuration;
using ScriptCs.Rebus.Scripts;

namespace ScriptCs.Rebus.Extensions
{
	public static class WebApiExtensions
	{
		public static WebApiScriptConfiguration AWebApiController(this ScriptConfiguration scriptConfiguration, string controllerName = null)
		{
			return new WebApiScriptConfiguration(scriptConfiguration, controllerName);
		}
	}

	public class WebApiScriptConfiguration
	{
		private readonly ScriptConfiguration _scriptConfiguration;
		private readonly string _controllerName;

		public WebApiScriptConfiguration(ScriptConfiguration scriptConfiguration, string controllerName)
		{
			_scriptConfiguration = scriptConfiguration;
			_controllerName = controllerName;
		}

		public ScriptConfiguration AsAScript(string script)
		{
			return _scriptConfiguration.Configuration(
				() => _scriptConfiguration.Bus.Send(new WebApiControllerScript
				{
					ControllerName = _controllerName,
					ScriptContent = script,
					NuGetDependencies = _scriptConfiguration.NugetDependencies.ToArray(),

					Namespaces = _scriptConfiguration.Namespaces.ToArray(),
					LocalDependencies = _scriptConfiguration.LocalDependencies.ToArray(),
					UseMono = _scriptConfiguration.UseMonoVar,
					LogLevel = _scriptConfiguration.GetLogLevel()
				}));
		}

		public ScriptConfiguration AsAScriptFile(string scriptFile)
		{
			return _scriptConfiguration.Configuration(
				() => _scriptConfiguration.Bus.Send(new WebApiControllerScript
				{
					ControllerName = _controllerName,
					ScriptContent = File.ReadAllText(scriptFile),
					NuGetDependencies = _scriptConfiguration.NugetDependencies.ToArray(),

					Namespaces = _scriptConfiguration.Namespaces.ToArray(),
					LocalDependencies = _scriptConfiguration.LocalDependencies.ToArray(),
					UseMono = _scriptConfiguration.UseMonoVar,
					LogLevel = _scriptConfiguration.GetLogLevel()
				}));
		}
	}
}