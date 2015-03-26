using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Dispatcher;
using NuGet;
using Rebus;
using Rebus.Configuration;
using Rebus.Transports.Msmq;
using ScriptCs.Contracts;
using ScriptCs.Rebus.Scripts;

namespace ScriptCs.Rebus.Hosting.ScriptHandlers.WebApi
{
	public class WebApiControllerBuilder
	{
		private BuiltinContainerAdapter _builtinContainerAdapter;

		public void Build(HttpConfiguration config)
		{
			if (config == null) throw new ArgumentNullException("config");

			if (Directory.Exists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory,
					"bin\\Scripts")))
			{

				var scripts =
					Directory.GetFiles(Path.Combine(AppDomain.CurrentDomain.BaseDirectory,
						"bin\\Scripts"))
						.Select(x => new FileInfo(x))
						.Where(x => x.Extension == ".csx")
						.Where(x => File.Exists(x.FullName + ".metadata"));

				IList<Type> controllers = new List<Type>();

				foreach (var script in scripts)
				{

					var metadata =
						File.ReadAllLines(script.FullName + ".metadata")[0].Split(':');

					var replyAddress = metadata[0].Trim();
					var transport = metadata[1].Trim();
					var logLevel = ToLogLevel(metadata[2].Trim());
					var bus = CreateReplyBus(transport, logLevel);

					ScriptExecutor.Init(CreateExecutableScript(script, logLevel),
						reply => bus.Publish(reply));

					ScriptExecutor.ScriptServicesBuilder.FileSystem<FileSystem>();
					ScriptExecutor.ScriptServicesBuilder
						.FilePreProcessor<WebApiFilePreProcessor>();

					var result = ScriptExecutor.ExecuteFile(script.FullName);

					var resultType = result.ReturnValue as Type;
					if (resultType != null)
					{
						if (resultType.IsSubclassOf(typeof(ApiController)))
						{
							controllers.Add(resultType);
						}
					}
				}

				// Add existing controllers
				var entryAssembly = Assembly.GetCallingAssembly();
				var existingControllers = entryAssembly.GetTypes()
					.Where(x => typeof(IHttpController).IsAssignableFrom(x))
					.ToList();

				controllers.AddRange(existingControllers);

				var controllerResolver = new AssemblyControllerTypeResolver(controllers);

				config.Services.Replace(typeof(IHttpControllerTypeResolver), controllerResolver);
			}
		}

		private LogLevel ToLogLevel(string logLevelAsString)
		{
			switch (logLevelAsString)
			{
				case "TRACE":
					return LogLevel.Trace;
				case "DEBUG":
					return LogLevel.Debug;
				case "ERROR":
					return LogLevel.Error;
				default:
					return LogLevel.Info;
			}
		}

		private IBus CreateReplyBus(string transport, LogLevel logLevel)
		{
			Action<RebusTransportConfigurer> transportConfig;
			switch (transport)
			{
				default:
					transportConfig = configurer => configurer.UseMsmqInOneWayClientMode();
					break;
			}

			_builtinContainerAdapter = new BuiltinContainerAdapter();
			var replyBus = Configure.With(_builtinContainerAdapter)
				.Transport(transportConfig)
				.CreateBus()
				.Start();

			return replyBus;
		}

		private WebApiControllerScript CreateExecutableScript(FileInfo script, LogLevel logLevel)
		{
			return new WebApiControllerScript
			{
				ScriptContent = File.ReadAllText(script.FullName),
				LocalDependencies = new[] { Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "bin", "System.Web.Http.dll"), "System.Net.Http" },
				NuGetDependencies = new string[0],
				Namespaces = new [] {"System.Web.Http", "System.Net.Http"},
				LogLevel = logLevel
			};
		}
	}
}