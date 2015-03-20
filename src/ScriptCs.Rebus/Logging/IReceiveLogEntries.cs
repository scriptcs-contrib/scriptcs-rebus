using ScriptCs.Contracts;

namespace ScriptCs.Rebus.Logging
{
	public interface IReceiveLogEntries
	{
		void Log(string logEntry);
		//LogLevel LogLevel { get; set; }
		LogLevel LogLevel { get; set; }
	}
}