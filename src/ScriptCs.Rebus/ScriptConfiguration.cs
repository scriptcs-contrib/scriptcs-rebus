using System.Collections.Generic;
using ScriptCs.Rebus.Scripts;

namespace ScriptCs.Rebus
{
    public class ScriptConfiguration
    {
        private readonly BaseBus _baseBus;
        private readonly string _script;
        private readonly List<string> _namespaces;
        private readonly List<string> _nugetDependencies;
        private readonly List<string> _localDependencies;
        private bool _useMono;
        private bool _useLogging;

        public ScriptConfiguration(BaseBus baseBus, string script)
        {
            _baseBus = baseBus;
            _script = script;
            _namespaces = new List<string>();
            _nugetDependencies = new List<string>();
            _localDependencies = new List<string>();
            _useMono = false;
            _useLogging = false;
        }

        public ScriptConfiguration ImportNamespace(string namespaceName)
        {
            _namespaces.Add(namespaceName);
            return this;
        }

        public ScriptConfiguration AddFromNuGet(string nugetDependency)
        {
            _nugetDependencies.Add(nugetDependency);
            return this;
        }

        public ScriptConfiguration AddLocal(string localDependency)
        {
            _localDependencies.Add(localDependency);
            return this;
        }

        public ScriptConfiguration UseMono()
        {
            _useMono = true;
            return this;
        }

        public ScriptConfiguration UseLogging()
        {
            _useLogging = true;
            return this;
        }

        public void Send()
        {
            _baseBus.Send(new DefaultExecutionScript
            {
                ScriptContent = _script,
                NuGetDependencies = _nugetDependencies.ToArray(),
                Namespaces = _namespaces.ToArray(),
                LocalDependencies = _localDependencies.ToArray(),
                UseMono = _useMono,
                UseLogging = _useLogging
            });
        }
    }
}