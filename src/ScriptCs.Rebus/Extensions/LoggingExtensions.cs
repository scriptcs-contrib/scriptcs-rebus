using System;
using System.Runtime.InteropServices;
using ScriptCs.Contracts;
using ScriptCs.Rebus.Configuration;
using ScriptCs.Rebus.Logging;

namespace ScriptCs.Rebus.Extensions
{
	public static class LoggingExtensions
	{
		public static ScriptConfiguration To<T>(this ScriptLoggingConfiguration loggingConfiguration, LogLevel logLevel = LogLevel.Info)
			where T : IReceiveLogEntries, new()
		{
			if (loggingConfiguration == null)
				throw new ArgumentNullException("loggingConfiguration");

			return loggingConfiguration.Logger(new T(), logLevel);
		}

		public static ScriptConfiguration ToConsole(
			this ScriptLoggingConfiguration loggingConfiguration, LogLevel logLevel = LogLevel.Info)
		{
			if (loggingConfiguration == null)
				throw new ArgumentNullException("loggingConfiguration");

			return loggingConfiguration.Logger(new ConsoleLogger(logLevel));
		}
	}
}