using Autofac;
using Common.Logging;
using Common.Logging.Simple;
using NuGet;
using ScriptCs.Contracts;
using ScriptCs.Engine.Roslyn;
using ScriptCs.Hosting.Package;
using IFileSystem = ScriptCs.Contracts.IFileSystem;

namespace ScriptCs.Rebus.Hosting
{
    // AutoFac configuration
    public class ScriptModule : Autofac.Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder
                .RegisterType<FileSystem>()
                .As<IFileSystem>()
                .SingleInstance();

            builder
                .RegisterType<ConsoleOutLogger>()
                .As<ILog>()
                .SingleInstance()
                .WithParameter("logName", @"Custom ScriptCs from C#")
                .WithParameter("logLevel", Common.Logging.LogLevel.All)
                .WithParameter("showLevel", true)
                .WithParameter("showDateTime", true)
                .WithParameter("showLogName", true)
                .WithParameter("dateTimeFormat", @"yyyy-mm-dd hh:mm:ss");

            builder
                .RegisterType<FilePreProcessor>()
                .As<IFilePreProcessor>()
                .SingleInstance();

            builder
                .RegisterType<ScriptHostFactory>()
                .As<IScriptHostFactory>()
                .SingleInstance();

            builder
                .RegisterType<RoslynScriptEngine>()
                .As<IScriptEngine>();

            builder
                .RegisterType<ScriptCs.ScriptExecutor>()
                .As<IScriptExecutor>();

            builder
                .RegisterType<NugetInstallationProvider>()
                .As<IInstallationProvider>()
                .SingleInstance();

            builder
                .RegisterType<PackageAssemblyResolver>()
                .As<IPackageAssemblyResolver>()
                .SingleInstance();

            builder
                .RegisterType<PackageContainer>()
                .As<IPackageContainer>()
                .SingleInstance();

            builder
                .RegisterType<PackageInstaller>()
                .As<IPackageInstaller>()
                .SingleInstance();

            builder
                .RegisterType<PackageManager>()
                .As<IPackageManager>()
                .SingleInstance();

            builder
                .RegisterType<ScriptPackResolver>()
                .As<IScriptPackResolver>()
                .SingleInstance();

            builder
                .RegisterType<ScriptExecutor>();
        }
    }
}