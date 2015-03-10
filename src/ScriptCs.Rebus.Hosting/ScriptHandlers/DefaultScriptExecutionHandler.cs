using System;
using Rebus;
using Rebus.Configuration;
using Rebus.Transports.Msmq;
using ScriptCs.Rebus.Scripts;

namespace ScriptCs.Rebus.Hosting.ScriptHandlers
{
	public class DefaultScriptExecutionHandler : IHandleMessages<DefaultExecutionScript>
	{
		public void Handle(DefaultExecutionScript message)
		{
			var bus = CreateReplyBus(MessageContext.GetCurrent().Headers["transport"].ToString());

			ScriptExecutor.Init(message, reply => bus.Advanced.Routing.Send(MessageContext.GetCurrent().ReturnAddress, reply));

			ScriptExecutor.ExecuteScript();
		}

		private IBus CreateReplyBus(string transport)
		{
			Action<RebusTransportConfigurer> transportConfig;
			switch (transport)
			{
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