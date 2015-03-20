using System;
using ScriptCs.Contracts;

namespace ScriptCs.Rebus.Logging
{
	public class ConsoleLogger : IReceiveLogEntries
	{
		public ConsoleLogger(LogLevel logLevel)
		{
			LogLevel = logLevel;
		}

		public void Log(string logEntry)
		{
			Console.WriteLine(logEntry);
		}

		public LogLevel LogLevel { set; get; }
	}

	//public abstract class BaseLogHandler
	//{
	//	public string Endpoint { set; private get; }
	//	public LogLevel LogLevel { get; private set; }

	//	protected BaseLogHandler(LogLevel logLevel)
	//	{
	//		LogLevel = logLevel;
	//		var container = new BuiltinContainerAdapter();

	//		//container.Handle<InfoLevelLogEntry>()


	//	}
	//}
	public class ScriptExecutionConsoleOutput
	{
		private readonly string _output;

		public ScriptExecutionConsoleOutput(string output)
		{
			_output = output;
		}

		public string Output
		{
			get { return _output; }
		}
	}

	public class ScriptExecutionLogEntry
	{
		private readonly string _logEntry;
		private readonly LogLevel _logLevel;

		public ScriptExecutionLogEntry(string logEntry, LogLevel logLevel)
		{
			_logEntry = logEntry;
			_logLevel = logLevel;
		}

		public string LogEntry
		{
			get { return _logEntry; }
		}

		public LogLevel Level
		{
			get { return _logLevel; }
		}
	}

	public class ScriptExecutionLifetimeStatus
	{
		public ScriptExecutionLifetime ExecutionLifetime { get; set; }
	}

}