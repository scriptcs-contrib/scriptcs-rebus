using System;
using Rebus;
using Rebus.AzureServiceBus;
using Rebus.Configuration;
using Rebus.Logging;
using Rebus.Serialization.Json;
using ScriptCs.Rebus.Configuration;
using ScriptCs.Rebus.Scripts;

namespace ScriptCs.Rebus.AzureServiceBus
{
	public class AzureServiceBus : BaseBus
	{
		private readonly string _azureConnectionString;
		private Action<LoggingConfigurer> _loggingConfigurer;

		public AzureServiceBus(string endpoint, string azureConnectionString)
		{
			Guard.AgainstNullArgument("endpoint", endpoint);
			Guard.AgainstNullArgument("azureConnectionString", azureConnectionString);

			Endpoint = endpoint;
			_azureConnectionString = azureConnectionString;
			_loggingConfigurer = configurer => configurer.None();

			Container = new BuiltinContainerAdapter();
		}

		public void RegisterHandler(IHandleMessages messageHandler)
		{
			Container.Register(() => messageHandler);
		}

		public override void Send<T>(T message)
        {
            Guard.AgainstNullArgument("message", message);

			var isAScript = typeof(IExecutionScript).IsAssignableFrom(message.GetType());

			if (SendBus == null)
            {
                ConfigureAzureSendBus(isAScript);
            }

            Guard.AgainstNullArgument("_sendBus", SendBus);

			// Add header information
			if (isAScript)
			{
				SendBus.AttachHeader(message, "connectionString", _azureConnectionString);
				SendBus.AttachHeader(message, "transport", "AZURE");
			}
			
			Console.Write("Sending message of type {0}...", message.GetType().Name);

			try
			{
				SendBus.Send(message);
			}
			catch (Exception e)
			{
				Console.WriteLine(e);
			}
		    finally
		    {
			    ShutDown();
		    }
            
			Console.WriteLine("sent.");

			ShutDown();
        }

		public override BaseBus Receive<T>(Action<T> action)
		{
			Guard.AgainstNullArgument("action", action);

			KnownTypes[typeof(T).Name] = typeof(T);
			Container.Handle(action);

			return this;
		}

		public override void Start()
		{
			if (ReceiveBus == null)
			{
				ConfigureAzureReceiveBus();
			}

			Console.WriteLine("Awaiting messsage on {0}...", Endpoint);
		}

		public override BaseBus UseLogging()
		{
			_loggingConfigurer = configurer => configurer.Console();

			return this;
		}

		private void ConfigureAzureSendBus(bool isAScript)
		{
			Action<RebusTransportConfigurer> transportConfig;
			if (!isAScript)
			{
				transportConfig = configurer => configurer.UseAzureServiceBusInOneWayClientMode(_azureConnectionString);
			}
			else
			{
				transportConfig =
					configurer =>
						configurer.UseAzureServiceBus(_azureConnectionString, string.Format("{0}.reply", Endpoint),
							string.Format("{0}.reply.error", Endpoint));
			}

			SendBus = Configure.With(Container)
				.Logging(_loggingConfigurer)
				.MessageOwnership(ownership => ownership.Use(new ScriptedOwnership(Endpoint)))
				.Serialization(serializer => serializer.UseJsonSerializer()
					.AddNameResolver(
						x => x.Assembly.GetName().Name.Contains("ℛ")
							? new TypeDescriptor("ScriptCs.Compiled", x.Name)
							: null))
						.Transport(transportConfig)
				.CreateBus()
				.Start();
		}

		private void ConfigureAzureReceiveBus()
		{
			ReceiveBus = Configure.With(Container)
				.Logging(_loggingConfigurer)
				.Serialization(serializer => serializer.UseJsonSerializer()
					.AddTypeResolver(x => x.AssemblyName == "ScriptCs.Compiled" ? KnownTypes[x.TypeName] : null))
				.Transport(
					configurer =>
						configurer.UseAzureServiceBus(_azureConnectionString, Endpoint, string.Format("{0}.error", Endpoint)))
				.CreateBus()
				.Start();

		}
	}
}
