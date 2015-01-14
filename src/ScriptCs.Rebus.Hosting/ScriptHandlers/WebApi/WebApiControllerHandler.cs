using System;
using System.IO;
using System.Web.Http;
using Rebus;
using ScriptCs.Rebus.Scripts;

namespace ScriptCs.Rebus.Hosting.ScriptHandlers.WebApi
{
    public class WebApiControllerHandler : IHandleMessages<WebApiControllerScript>
    {
	    public WebApiControllerHandler(HttpConfiguration config)
	    {
	    }

        public void Handle(WebApiControllerScript message)
        {
			//TODO: Make sure message.Name has a valid name

			var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, string.Format("bin\\Scripts\\{0}Controller.csx", message.Name));
			File.WriteAllText(path, message.ScriptContent);

			var executor = new ScriptExecutor(message);
	        executor.ScriptServicesBuilder.FilePreProcessor<WebApiFilePreProcessor>
		        ();
			executor.Execute();

	        // Make it possible to use existing controllers
	        // Make it possible to use existing models

	        // Write scripted controllers into Controllers folder
	        // Write scripted models into Models folder

        }
    }
}