using System;
using System.IO;
using Rebus;
using Rebus.Configuration;
using Rebus.Transports.Msmq;
using ScriptCs.Rebus.Scripts;

namespace ScriptCs.Rebus.Hosting.ScriptHandlers.WebApi
{
    public class ReceiveWebApiControllerHandler : IHandleMessages<WebApiControllerScript>
    {
	    public void Handle(WebApiControllerScript message)
	    {
		    var bus =
			    CreateReplyBus(
				    MessageContext.GetCurrent().Headers["transport"].ToString());
			
			bus.Advanced.Routing.Send(MessageContext.GetCurrent().ReturnAddress, "Script received...");

		    if (message == null) throw new ArgumentNullException("message");

		    if (message.ControllerName != null && message.ControllerName.ToLower().EndsWith("controller"))
	        {
		        message.ControllerName = message.ControllerName.Substring(0,
			        message.ControllerName.IndexOf("controller"));
	        }

		    if (string.IsNullOrWhiteSpace(message.ControllerName))
		    {
			    message.ControllerName = "Scripted";
		    }

		    Directory.CreateDirectory(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "bin", "Scripts"));
		    var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, string.Format("bin\\Scripts\\{0}Controller.csx", message.ControllerName));
		    File.WriteAllText(path, message.ScriptContent);

		    var metaDataPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory,
			    string.Format("bin\\Scripts\\{0}Controller.csx.metadata", message.ControllerName));
		    File.WriteAllText(metaDataPath,
			    string.Format("{0} : {1}", MessageContext.GetCurrent().ReturnAddress,
				    MessageContext.GetCurrent().Headers["transport"]));

			bus.Advanced.Routing.Send(MessageContext.GetCurrent().ReturnAddress, "Script saved...");
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