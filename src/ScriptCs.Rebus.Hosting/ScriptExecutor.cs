using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Versioning;
using ScriptCs.Contracts;
using Common.Logging;
using ScriptCs.Engine.Mono;
using ScriptCs.Engine.Roslyn;
using ScriptCs.Hosting;
using ScriptCs.Rebus.Hosting.ScriptHandlers.WebApi;
using ScriptCs.Rebus.Scripts;
using LogLevel = ScriptCs.Contracts.LogLevel;

namespace ScriptCs.Rebus.Hosting
{
    public class ScriptExecutor
    {
	    private static DefaultExecutionScript _executionScript;
	    public static ScriptServicesBuilder ScriptServicesBuilder;
	    private static IScriptExecutor _scriptExecutor;

	    public static void Init(DefaultExecutionScript executionScript)
	    {
		    if (executionScript == null)
			    throw new ArgumentNullException("executionScript");

		    _executionScript = executionScript;
		    CreateScriptServices(executionScript.UseMono,
				executionScript.UseLogging);
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
		    if (_executionScript.UseLogging && scriptResult != null)
			    if (scriptResult.CompileExceptionInfo != null)
				    if (scriptResult.CompileExceptionInfo.SourceException != null)
					    scriptServices.Logger.Debug(
						    scriptResult.CompileExceptionInfo.SourceException.Message);
		    _scriptExecutor.Terminate();

		    return scriptResult;
	    }

	    private static ScriptServices SetupExecution()
	    {
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

		    var scriptPacks = scriptPackResolver.GetPacks();
		    _scriptExecutor.Initialize(assemblyPaths, scriptPacks.Union(new List<IScriptPack>() { new WebApiScriptHack() }));
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
			//TODO: Make sure we're in some bin directory, or leave out existing

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

        private static void CreateScriptServices(bool useMono, bool useLogging)
        {
            var console = new ScriptConsole();
            var configurator = new LoggerConfigurator(useLogging ? LogLevel.Debug : LogLevel.Info);
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