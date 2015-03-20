using System;
using ScriptCs.Contracts;
using ScriptCs.Rebus.Logging;

namespace ScriptCs.Rebus.Configuration
{
	public class ScriptLoggingConfiguration
	{
		private readonly ScriptConfiguration _scriptConfiguration;
		private readonly Action<IReceiveLogEntries> _addLogEntryHandler;

		internal ScriptLoggingConfiguration(ScriptConfiguration scriptConfiguration, Action<IReceiveLogEntries> addLogEntryHandler)
		{
			if (scriptConfiguration == null)
				throw new ArgumentNullException("scriptConfiguration");
			if (addLogEntryHandler == null)
				throw new ArgumentNullException("addLogEntryHandler");

			_scriptConfiguration = scriptConfiguration;
			_addLogEntryHandler = addLogEntryHandler;
		}

		//public string Endpoint { get; set; }

		public ScriptConfiguration Logger(IReceiveLogEntries logEntryHandler)
		{
			_addLogEntryHandler(logEntryHandler);
			return _scriptConfiguration;
		}

		public ScriptConfiguration Logger(IReceiveLogEntries logEntryHandler, LogLevel logLevel)
		{
			logEntryHandler.LogLevel = logLevel;

			_addLogEntryHandler(logEntryHandler);
			return _scriptConfiguration;
		}


	}
}