using ScriptCs.Rebus.Configuration;

namespace ScriptCs.Rebus.Extensions
{
	public static class WebApiExtensions
	{
		public static WebApiScriptConfiguration AWebApiController(this ScriptConfiguration scriptConfiguration, string controllerName)
		{
			return new WebApiScriptConfiguration();
		}
	}

	public class WebApiScriptConfiguration
	{
		public ScriptConfiguration AsScript(ScriptConfiguration scriptConfiguration, string script)
		{
			return scriptConfiguration;
		}

		public ScriptConfiguration AsScriptFile(ScriptConfiguration scriptConfiguration, string scriptFile)
		{
			return scriptConfiguration;
		}
	}
}