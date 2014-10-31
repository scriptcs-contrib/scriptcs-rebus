using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mime;
using System.Reflection;
using System.Runtime.Remoting.Channels;
using System.Runtime.Versioning;
using System.Security.Cryptography;
using Common.Logging.Simple;
using ICSharpCode.NRefactory.CSharp.Refactoring;
using ScriptCs.Contracts;
using Common.Logging;
using ScriptCs.Engine.Mono;
using ScriptCs.Engine.Roslyn;
using ScriptCs.Hosting;
using ScriptCs.Hosting.Package;
using LogLevel = ScriptCs.Contracts.LogLevel;

namespace ScriptCs.Rebus.Hosting
{
    public class ScriptExecutor
    {
        public void Execute(Script script)
        {
            //// set current dicrectory, import for NuGet.
            Environment.CurrentDirectory = AppDomain.CurrentDomain.BaseDirectory;

            var services = CreateScriptServices(script.UseMono, script.UseLogging);
            var scriptExecutor = services.Executor;
            var scriptPackResolver = services.ScriptPackResolver;
            services.InstallationProvider.Initialize();

            // prepare NuGet dependencies, download them if required
            var assemblyPaths = PreparePackages(services.PackageAssemblyResolver,
                services.PackageInstaller, PrepareAdditionalPackages(script.NuGetDependencies), script.LocalDependencies, services.Logger);

            scriptExecutor.Initialize(assemblyPaths, scriptPackResolver.GetPacks());
            scriptExecutor.ImportNamespaces(script.Namespaces);
            scriptExecutor.AddReferences(script.LocalDependencies);

            var scriptResult = scriptExecutor.ExecuteScript(script.ScriptContent, "");

            if (script.UseLogging && scriptResult != null)
                if (scriptResult.CompileExceptionInfo != null)
                    if (scriptResult.CompileExceptionInfo.SourceException != null)
                        services.Logger.Debug(scriptResult.CompileExceptionInfo.SourceException.Message);
            scriptExecutor.Terminate();
        }

        private IEnumerable<IPackageReference> PrepareAdditionalPackages(IEnumerable<string> dependencies)
        {
            return from dep in dependencies
                   select new PackageReference(dep, new FrameworkName(".NETFramework,Version=v4.0"), string.Empty);
        }

        // prepare NuGet dependencies, download them if required
        private static IEnumerable<string> PreparePackages(IPackageAssemblyResolver packageAssemblyResolver, IPackageInstaller packageInstaller, IEnumerable<IPackageReference> additionalNuGetReferences, IEnumerable<string> localDependencies, ILog logger)
        {
            var workingDirectory = Environment.CurrentDirectory;
            //var locals = localDependencies.Select(localDependency => Path.Combine(workingDirectory, localDependency)).ToList();

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

        private ScriptServices CreateScriptServices(bool useMono, bool useLogging)
        {
            var console = new ScriptConsole();
            var configurator = new LoggerConfigurator(useLogging ? LogLevel.Debug : LogLevel.Info);
            configurator.Configure(console);
            var logger = configurator.GetLogger();
            var builder = new ScriptServicesBuilder(console, logger);

            if (useMono)
            {
                builder.ScriptEngine<MonoScriptEngine>();
            }
            else
            {
                builder.ScriptEngine<RoslynScriptEngine>();
            }

            builder.FileSystem<ScriptFileSystem>();
            return builder.Build();
        }
    }
    
    /// <summary>
    /// By default, the scriptcs.hosting inspects the bin folder for references.
    /// </summary>
    public class ScriptFileSystem : FileSystem
    {
        public override string BinFolder
        {
            get { return AppDomain.CurrentDomain.BaseDirectory; }
        }
    }


}