using System;
using Autofac;
using Autofac.Core.Registration;
using Common.Logging;
using Common.Logging.Simple;
using Rebus;
using ScriptCs.Contracts;
using ScriptCs.Engine.Mono;
using ScriptCs.Hosting;
using ScriptCs.Rebus;

namespace ScriptCs.Rebus.Hosting
{
    public class ScriptHandler : IHandleMessages<Script>
    {
        public void Handle(Script message)
        {
            var executor = new ScriptExecutor();
            executor.Execute(message);
        }
    }
}