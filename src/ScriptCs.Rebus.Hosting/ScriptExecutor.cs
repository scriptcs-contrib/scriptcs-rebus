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
            // set directory to where script is
            // required to find NuGet dependencies
            //Environment.CurrentDirectory = "..\\..\\..";
            
            // prepare NuGet dependencies, download them if required
            var nuGetReferences = PreparePackages(_fileSystem, _packageAssemblyResolver,
                                            _packageInstaller, PrepareAdditionalPackages(dependencies));
            
            // get script packs: not fully tested yet        
            var scriptPacks = _scriptPackResolver.GetPacks();

            // execute script from file
            _scriptExecutor.Initialize(nuGetReferences, scriptPacks);
            _scriptExecutor.ExecuteScript(script);
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
            Console.WriteLine("Working folder: {0}",workingDirectory);
            var binDirectory = Path.Combine(workingDirectory, fileSystem.BinFolder);

            var packages = packageAssemblyResolver.GetPackages(workingDirectory);
            packages = packages.Concat(additionalReferences);

            Console.WriteLine("Number of packages: {0}", packages.Count());

            packageInstaller.InstallPackages(
                                packages,
                                allowPreRelease: true);

            return packageAssemblyResolver.GetAssemblyNames(workingDirectory);

            // current implementeation of RoslynCTP required dependencies to be in 'bin' folder
            //if (!fileSystem.DirectoryExists(binDirectory))
            //{
            //    fileSystem.CreateDirectory(binDirectory);
            //}

            //// copy dependencies one by one from 'packages' to 'bin'
            //foreach (var assemblyName
            //            in packageAssemblyResolver.GetAssemblyNames(workingDirectory))
            //{
            //    var assemblyFileName = Path.GetFileName(assemblyName);
            //    var destFile = Path.Combine(binDirectory, assemblyFileName);

            //    var sourceFileLastWriteTime = fileSystem.GetLastWriteTime(assemblyName);
            //    var destFileLastWriteTime = fileSystem.GetLastWriteTime(destFile);

            //    if (sourceFileLastWriteTime == destFileLastWriteTime)
            //    {
            //        outputCallback(string.Format("Skipped: '{0}' because it is already exists", assemblyName));
            //    }
            //    else
            //    {
            //        fileSystem.Copy(assemblyName, destFile, overwrite: true);

            //        if (outputCallback != null)
            //        {
            //            outputCallback(string.Format("Copy: '{0}' to '{1}'", assemblyName, destFile));
            //        }
            //    }

            //    yield return destFile;
            //}
        }

    }
}