using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Versioning;
using Common.Logging;
using ScriptCs.Contracts;
using ScriptCs.Engine.Mono;
using ScriptCs.Engine.Roslyn;
using ScriptCs.Hosting;
using ScriptCs.Rebus.Configuration;
using ScriptCs.Rebus.Hosting.Extensions;
using ScriptCs.Rebus.Logging;
using LogLevel = ScriptCs.Contracts.LogLevel;

namespace ScriptCs.Rebus.Hosting
{
    public class ScriptExecutor
    {
	    private static IExecutionScript _executionScript;
	    public static ScriptServicesBuilder ScriptServicesBuilder;
	    private static IScriptExecutor _scriptExecutor;
	    private static Action<object> _reply; 

	    public static void Init(IExecutionScript executionScript, Action<object> reply)
	    {
		    if (executionScript == null)
			    throw new ArgumentNullException("executionScript");
		    if (reply == null) throw new ArgumentNullException("reply");

		    _executionScript = executionScript;
		    _reply = reply;
			CreateScriptServices(executionScript.UseMono,
				executionScript.LogLevel, reply);
	    }
		
	    public static ScriptResult ExecuteFile(string filePath)
	    {
		    var scriptServices = SetupExecution();

		    var scriptResult = _scriptExecutor.Execute(filePath, "");

		    return EvaluateScriptResult(scriptResult, scriptServices);
	    }

	    public static ScriptResult ExecuteScript()
        {
			var scriptServices = SetupExecution();

		    var scriptResult = _scriptExecutor.ExecuteScript(_executionScript.ScriptContent, "");

            return EvaluateScriptResult(scriptResult, scriptServices);
        }

	    private static ScriptResult EvaluateScriptResult(ScriptResult scriptResult,
		    ScriptServices scriptServices)
	    {
		    if (scriptResult != null)
			    if (scriptResult.CompileExceptionInfo != null)
				    if (scriptResult.CompileExceptionInfo.SourceException != null)
					    scriptServices.Logger.Debug(
						    scriptResult.CompileExceptionInfo.SourceException.Message);
		    _scriptExecutor.Terminate();

			_reply(new ScriptExecutionLifetimeStatus {ExecutionLifetime = ScriptExecutionLifetime.Terminated});
			
			return scriptResult;
	    }

	    private static ScriptServices SetupExecution()
	    {
			_reply(new ScriptExecutionLifetimeStatus {ExecutionLifetime = ScriptExecutionLifetime.Started});

		    var scriptServices = ScriptServicesBuilder.Build();
		    
			// set current dicrectory, import for NuGet.
		    Environment.CurrentDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, scriptServices.FileSystem.BinFolder);
		    

		    _scriptExecutor = scriptServices.Executor;
		    var scriptPackResolver = scriptServices.ScriptPackResolver;
		    scriptServices.InstallationProvider.Initialize();

		    // prepare NuGet dependencies, download them if required
		    var assemblyPaths = PreparePackages(scriptServices.PackageAssemblyResolver,
			    scriptServices.PackageInstaller,
			    PrepareAdditionalPackages(_executionScript.NuGetDependencies),
			    _executionScript.LocalDependencies, scriptServices.Logger);

		    _scriptExecutor.Initialize(assemblyPaths, scriptPackResolver.GetPacks());
		    _scriptExecutor.ImportNamespaces(_executionScript.Namespaces);
		    _scriptExecutor.AddReferences(_executionScript.LocalDependencies);
		    return scriptServices;
	    }

	    private static IEnumerable<IPackageReference> PrepareAdditionalPackages(IEnumerable<string> dependencies)
        {
            return from dep in dependencies
                   select new PackageReference(dep, new FrameworkName(".NETFramework,Version=v4.0"), string.Empty);
        }

        // prepare NuGet dependencies, download them if required
        private static IEnumerable<string> PreparePackages(IPackageAssemblyResolver packageAssemblyResolver, IPackageInstaller packageInstaller, IEnumerable<IPackageReference> additionalNuGetReferences, IEnumerable<string> localDependencies, ILog logger)
        {
	        var workingDirectory = Environment.CurrentDirectory;

            var packages = packageAssemblyResolver.GetPackages(workingDirectory);
            packages = packages.Concat(additionalNuGetReferences);

            try
            {
                packageInstaller.InstallPackages(
                    packages, true);
            }
            catch (Exception e)
            {
                logger.ErrorFormat("Installation failed: {0}.", e.Message);
            }
            var assemblyNames = packageAssemblyResolver.GetAssemblyNames(workingDirectory);
			assemblyNames = assemblyNames.Concat(localDependencies);
            return assemblyNames;
        }

        private static void CreateScriptServices(bool useMono, LogLevel logLevel, Action<object> reply)
        {
            var console = new MessagingConsole(reply);
            var configurator = new LoggerConfigurator(logLevel);
            configurator.Configure(console);
            var logger = configurator.GetLogger();
            ScriptServicesBuilder = new ScriptServicesBuilder(console, logger);

            if (useMono)
            {
                ScriptServicesBuilder.ScriptEngine<MonoScriptEngine>();
            }
            else
            {
                ScriptServicesBuilder.ScriptEngine<RoslynScriptEngine>();
            }

			ScriptServicesBuilder.FileSystem<ScriptFileSystem>();
        }
    }
}