using System;
using System.IO;
using Rebus;
using ScriptCs.Rebus.Scripts;

namespace ScriptCs.Rebus.Hosting.ScriptHandlers.WebApi
{
    public class WebApiControllerHandler : IHandleMessages<WebApiControllerScript>
    {
	    public WebApiControllerHandler()
	    {
	    }

	    public void Handle(WebApiControllerScript message)
        {
			//TODO: Split script execution and script saving...!

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
			

			// Make it possible to use existing controllers
	        // Make it possible to use existing models

	        // Write scripted controllers into Controllers folder
	        // Write scripted models into Models folder

        }
    }
}