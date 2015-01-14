using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Versioning;
using ScriptCs.Contracts;
using Common.Logging;
using ScriptCs.Engine.Mono;
using ScriptCs.Engine.Roslyn;
using ScriptCs.Hosting;
using ScriptCs.Rebus.Scripts;
using LogLevel = ScriptCs.Contracts.LogLevel;

namespace ScriptCs.Rebus.Hosting
{
    public class ScriptExecutor
    {
	    private readonly DefaultExecutionScript _executionScript;
	    public ScriptServicesBuilder ScriptServicesBuilder;

	    public ScriptExecutor(DefaultExecutionScript executionScript)
	    {
		    if (executionScript == null)
			    throw new ArgumentNullException("executionScript");

		    _executionScript = executionScript;
		    CreateScriptServices(executionScript.UseMono,
				executionScript.UseLogging);
	    }

	    public void Execute()
        {
			// set current dicrectory, import for NuGet.
			Environment.CurrentDirectory = AppDomain.CurrentDomain.BaseDirectory;

		    var scriptServices = ScriptServicesBuilder.Build();

			var scriptExecutor = scriptServices.Executor;
			var scriptPackResolver = scriptServices.ScriptPackResolver;
			scriptServices.InstallationProvider.Initialize();

			// prepare NuGet dependencies, download them if required
			var assemblyPaths = PreparePackages(scriptServices.PackageAssemblyResolver,
				scriptServices.PackageInstaller,
				PrepareAdditionalPackages(_executionScript.NuGetDependencies),
				_executionScript.LocalDependencies, scriptServices.Logger);

			scriptExecutor.Initialize(assemblyPaths, scriptPackResolver.GetPacks());
			scriptExecutor.ImportNamespaces(_executionScript.Namespaces);
			scriptExecutor.AddReferences(_executionScript.LocalDependencies);
			
			var scriptResult = scriptExecutor.ExecuteScript(_executionScript.ScriptContent, "");

            if (_executionScript.UseLogging && scriptResult != null)
                if (scriptResult.CompileExceptionInfo != null)
                    if (scriptResult.CompileExceptionInfo.SourceException != null)
                        scriptServices.Logger.Debug(scriptResult.CompileExceptionInfo.SourceException.Message);
            scriptExecutor.Terminate();
        }


	    private IEnumerable<IPackageReference> PrepareAdditionalPackages(IEnumerable<string> dependencies)
        {
            return from dep in dependencies
                   select new PackageReference(dep, new FrameworkName(".NETFramework,Version=v4.0"), string.Empty);
        }

        // prepare NuGet dependencies, download them if required
        private IEnumerable<string> PreparePackages(IPackageAssemblyResolver packageAssemblyResolver, IPackageInstaller packageInstaller, IEnumerable<IPackageReference> additionalNuGetReferences, IEnumerable<string> localDependencies, ILog logger)
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

        private void CreateScriptServices(bool useMono, bool useLogging)
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