using System;
using Rebus;
using Rebus.AzureServiceBus;
using Rebus.Configuration;
using Rebus.Messages;
using Rebus.RabbitMQ;
using Rebus.Transports.Msmq;
using ScriptCs.Rebus.Scripts;

namespace ScriptCs.Rebus.Hosting.ScriptHandlers
{
	public class DefaultScriptExecutionHandler : IHandleMessages<DefaultExecutionScript>
	{
		public void Handle(DefaultExecutionScript message)
		{
			var bus = CreateReplyBus(MessageContext.GetCurrent().Headers["transport"].ToString(), MessageContext.GetCurrent().Headers["connectionString"].ToString());

			ScriptExecutor.Init(message, reply => bus.Advanced.Routing.Send(MessageContext.GetCurrent().ReturnAddress, reply));

			ScriptExecutor.ExecuteScript();
		}

		private IBus CreateReplyBus(string transport, string connectionString)
		{
			Action<RebusTransportConfigurer> transportConfig = null;
			switch (transport)
			{
				case "AZURE":
					transportConfig =
						configurer => configurer.UseAzureServiceBusInOneWayClientMode(connectionString);
					break;
				case "RABBIT":
					transportConfig = configurer => configurer.UseRabbitMqInOneWayMode(connectionString);
					break;
				default:
					transportConfig = configurer => configurer.UseMsmqInOneWayClientMode();
					break;
			}

			return Configure.With(new BuiltinContainerAdapter())
				.Transport(transportConfig)
				.CreateBus()
				.Start();
		}

	}
}