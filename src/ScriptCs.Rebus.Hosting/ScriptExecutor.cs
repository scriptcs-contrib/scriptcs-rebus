using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mime;
using System.Reflection;
using System.Runtime.Versioning;
using System.Security.Cryptography;
using Common.Logging;
using ScriptCs.Contracts;

namespace ScriptCs.Rebus.Hosting
{
    public class ScriptExecutor
    {
        private readonly ILog _logger;
        private readonly IFileSystem _fileSystem;
        private readonly IPackageAssemblyResolver _packageAssemblyResolver;
        private readonly IPackageInstaller _packageInstaller;
        private readonly IScriptPackResolver _scriptPackResolver;
        private readonly IScriptExecutor _scriptExecutor;

        public ScriptExecutor(ILog logger, IFileSystem fileSystem,
                            IPackageAssemblyResolver packageAssemblyResolver,
                            IPackageInstaller packageInstaller, IScriptPackResolver scriptPackResolver,
                            IScriptExecutor scriptExecutor)
        {
            _logger = logger;
            _fileSystem = fileSystem;
            _packageAssemblyResolver = packageAssemblyResolver;
            _packageInstaller = packageInstaller;
            _scriptPackResolver = scriptPackResolver;
            _scriptExecutor = scriptExecutor;
        }

        public void Execute(string script, string[] dependencies)
        {
            // set current dicrectory, import for NuGet.
            Environment.CurrentDirectory = AppDomain.CurrentDomain.BaseDirectory;

            // prepare NuGet dependencies, download them if required
            var nuGetReferences = PreparePackages(_fileSystem, _packageAssemblyResolver,
                                            _packageInstaller, PrepareAdditionalPackages(dependencies));

            // get script packs: not fully tested yet        
            var scriptPacks = _scriptPackResolver.GetPacks();

            // execute script from file
            _scriptExecutor.Initialize(nuGetReferences, scriptPacks);
            var scriptResult = _scriptExecutor.ExecuteScript(script);
            if (scriptResult != null)
                if (scriptResult.CompileExceptionInfo != null)
                    if (scriptResult.CompileExceptionInfo.SourceException != null)
                        _logger.Debug(scriptResult.CompileExceptionInfo.SourceException.Message);
        }

        private IEnumerable<IPackageReference> PrepareAdditionalPackages(string[] dependencies)
        {
            return from dep in dependencies
                select new PackageReference(dep, new FrameworkName(".NETFramework,Version=v4.0"), string.Empty);
        }

        // prepare NuGet dependencies, download them if required
        private static IEnumerable<string> PreparePackages(IFileSystem fileSystem, IPackageAssemblyResolver packageAssemblyResolver,
                                IPackageInstaller packageInstaller, IEnumerable<IPackageReference> additionalReferences)
        {
            var workingDirectory = Environment.CurrentDirectory;

            var packages = packageAssemblyResolver.GetPackages(workingDirectory);
            packages = packages.Concat(additionalReferences);

            packageInstaller.InstallPackages(
                                packages,
                                allowPreRelease: true);

            return packageAssemblyResolver.GetAssemblyNames(workingDirectory);
        }

    }
}