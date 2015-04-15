using System;
using System.Collections.Generic;
using System.Linq;
using Rebus;
using Rebus.Configuration;
using Rebus.Logging;
using Rebus.Transports.Msmq;
using ScriptCs.Rebus.Logging;
using ScriptCs.Rebus.Scripts;
using LogLevel = ScriptCs.Contracts.LogLevel;

namespace ScriptCs.Rebus.Configuration
{
	public class ScriptConfiguration
	{
		internal readonly BaseBus Bus;
		private readonly string _endpoint;
		internal readonly List<string> Namespaces;
		internal readonly List<string> NugetDependencies;
		internal readonly List<string> LocalDependencies;
		private readonly List<IReceiveLogEntries> _logEntryHandlers;
		internal bool UseMonoVar;
		internal string ScriptContent;
		private Action _sendScript;

		public ScriptConfiguration(BaseBus bus, string endpoint)
		{
			if (bus == null) throw new ArgumentNullException("bus");
			if (endpoint == null) throw new ArgumentNullException("endpoint");
			Bus = bus;
			_endpoint = endpoint;
			Namespaces = new List<string>();
			NugetDependencies = new List<string>();
			LocalDependencies = new List<string>();
			_logEntryHandlers = new List<IReceiveLogEntries>();
			UseMonoVar = false;

			CreateLogBus();
		}

		public ScriptConfiguration ImportNamespace(string namespaceName)
		{
			Namespaces.Add(namespaceName);
			return this;
		}

		public ScriptConfiguration AddFromNuGet(string nugetDependency)
		{
			NugetDependencies.Add(nugetDependency);
			return this;
		}

		public ScriptConfiguration AddLocal(string localDependency)
		{
			LocalDependencies.Add(localDependency);
			return this;
		}

		public ScriptConfiguration UseMono()
		{
			UseMonoVar = true;
			return this;
		}

		public ScriptLoggingConfiguration Log
		{
			get
			{
				return new ScriptLoggingConfiguration(this, s => _logEntryHandlers.Add(s));
			}
		}

		public void Send()
		{
			_sendScript();
		}

		internal LogLevel GetLogLevel()
		{
			if (_logEntryHandlers.Any(x => x.LogLevel == LogLevel.Trace))
			{
				return LogLevel.Trace;
			}

			if (_logEntryHandlers.Any(x => x.LogLevel == LogLevel.Debug))
			{
				return LogLevel.Debug;
			}

			if (_logEntryHandlers.Any(x => x.LogLevel == LogLevel.Error))
			{
				return LogLevel.Error;
			}

			return LogLevel.Info;
		}

		private BuiltinContainerAdapter ConfigureContainer()
		{
			var container = new BuiltinContainerAdapter();

			container.Handle<ScriptExecutionLifetimeStatus>(
				message =>
				{
					switch (message.ExecutionLifetime)
					{
						case ScriptExecutionLifetime.Started:
							Console.WriteLine("Initiating script execution");
							break;
						case ScriptExecutionLifetime.Terminated:
							Console.WriteLine("Finished script execution...");
							//if (_logBus != null) _logBus.Dispose();
							Bus.ShutDown();
							break;
					}
				});

			container.Handle<ScriptExecutionConsoleOutput>(
				message =>
					_logEntryHandlers
						.ToList()
						.ForEach(x => x.Log(message.Output)));

			container.Handle<ScriptExecutionLogEntry>(
				message =>
				{
					switch (message.Level)
					{

						case LogLevel.Trace:
							_logEntryHandlers.Where(handler => handler.LogLevel == LogLevel.Trace)
								.ToList()
								.ForEach(x => x.Log(message.LogEntry));
							break;

						case LogLevel.Debug:
							_logEntryHandlers.Where(handler => handler.LogLevel == LogLevel.Debug ||
							                                   handler.LogLevel == LogLevel.Trace)
								.ToList()
								.ForEach(x => x.Log(message.LogEntry));
							break;

						case LogLevel.Error:
							_logEntryHandlers.Where(handler => handler.LogLevel == LogLevel.Error ||
							                                   handler.LogLevel == LogLevel.Debug ||
							                                   handler.LogLevel == LogLevel.Trace)
								.ToList()
								.ForEach(x => x.Log(message.LogEntry));
							break;

						default:
							_logEntryHandlers.ForEach(x => x.Log(message.LogEntry));
							break;
					}
				});

			return container;
		}

		private void CreateLogBus()
		{
			var container = ConfigureContainer();

			Configure.With(container)
				.Logging(configurer => configurer.None())
				.Transport(configurer =>
					configurer.UseMsmq(string.Format("{0}.reply", _endpoint),
						string.Format("{0}.reply.error", _endpoint)))
				.CreateBus()
				.Start();
		}

		internal ScriptConfiguration Configuration(Action executionScript)
		{
			_sendScript = executionScript;

			return this;
		}
	}

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
