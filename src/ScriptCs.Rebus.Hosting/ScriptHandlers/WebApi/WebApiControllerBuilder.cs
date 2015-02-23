using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Dispatcher;
using NuGet;
using ScriptCs.Rebus.Scripts;

namespace ScriptCs.Rebus.Hosting.ScriptHandlers.WebApi
{
	public class WebApiControllerBuilder
	{
		public void Build(HttpConfiguration config)
		{
			if (config == null) throw new ArgumentNullException("config");

			if (Directory.Exists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory,
					"bin\\Scripts")))
			{

				var scripts =
					Directory.GetFiles(Path.Combine(AppDomain.CurrentDomain.BaseDirectory,
						"bin\\Scripts"))
						.Where(x => new FileInfo(x).Extension == ".csx")
						.Select(x => new FileInfo(x));

				IList<Type> controllers = new List<Type>();

				foreach (var script in scripts)
				{
					ScriptExecutor.Init(CreateExecutableScript(script));

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
				var entryAssembly = Assembly.GetEntryAssembly();
				var existingControllers = entryAssembly.GetTypes()
					.Where(x => typeof(IHttpController).IsAssignableFrom(x))
					.ToList();

				controllers.AddRange(existingControllers);

				var controllerResolver = new AssemblyControllerTypeResolver(controllers);

				config.Services.Replace(typeof(IHttpControllerTypeResolver), controllerResolver);
			}
		}

		private DefaultExecutionScript CreateExecutableScript(FileInfo script)
		{
			return new WebApiControllerScript
			{
				ScriptContent = File.ReadAllText(script.FullName),
				LocalDependencies = new string[0],// { @"C:\Projects\Trash\SomeWebApp\SomeWebApp\bin\System.Web.Http.dll" },
				NuGetDependencies = new string[0],
				Namespaces = new string[0]
			};
		}
	}
}