using System;
using System.Collections.Generic;
using System.Linq;
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
		private readonly BaseBus _baseBus;
		private readonly string _script;
		private readonly string _endpoint;
		private readonly List<string> _namespaces;
		private readonly List<string> _nugetDependencies;
		private readonly List<string> _localDependencies;
		private readonly List<IReceiveLogEntries> _logEntryHandlers;
		private bool _useMono;
		private IExecutionScript _executionScript;

		public ScriptConfiguration(BaseBus baseBus, string script, string endpoint)
		{
			if (baseBus == null) throw new ArgumentNullException("baseBus");
			if (script == null) throw new ArgumentNullException("script");
			if (endpoint == null) throw new ArgumentNullException("endpoint");
			_baseBus = baseBus;
			_script = script;
			_endpoint = endpoint;
			_namespaces = new List<string>();
			_nugetDependencies = new List<string>();
			_localDependencies = new List<string>();
			_logEntryHandlers = new List<IReceiveLogEntries>();
			_useMono = false;

			CreateLogBus();
		}

		public ScriptConfiguration(BaseBus baseBus, IExecutionScript script,
			string endpoint) : this(baseBus, script.ScriptContent, endpoint)
		{
			_executionScript = script;
		}

		public ScriptConfiguration()
		{
			throw new NotImplementedException();
		}

		public ScriptConfiguration ImportNamespace(string namespaceName)
		{
			_namespaces.Add(namespaceName);
			return this;
		}

		public ScriptConfiguration AddFromNuGet(string nugetDependency)
		{
			_nugetDependencies.Add(nugetDependency);
			return this;
		}

		public ScriptConfiguration AddLocal(string localDependency)
		{
			_localDependencies.Add(localDependency);
			return this;
		}

		public ScriptConfiguration UseMono()
		{
			_useMono = true;
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
			_baseBus.Send(_executionScript);

			_baseBus.Send(new DefaultExecutionScript
			{
				ScriptContent = _script,
				NuGetDependencies = _nugetDependencies.ToArray(),

				Namespaces = _namespaces.ToArray(),
				LocalDependencies = _localDependencies.ToArray(),
				UseMono = _useMono,
				LogLevel = GetLogLevel(_logEntryHandlers)
			});
		}

		private LogLevel GetLogLevel(List<IReceiveLogEntries> logEntryHandlers)
		{
			if (logEntryHandlers.Any(x => x.LogLevel == LogLevel.Trace))
			{
				return LogLevel.Trace;
			}

			if (logEntryHandlers.Any(x => x.LogLevel == LogLevel.Debug))
			{
				return LogLevel.Debug;
			}

			if (logEntryHandlers.Any(x => x.LogLevel == LogLevel.Error))
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
							_baseBus.Dispose();
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