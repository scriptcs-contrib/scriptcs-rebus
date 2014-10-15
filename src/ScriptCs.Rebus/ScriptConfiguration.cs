using System.Collections.Generic;

namespace ScriptCs.Rebus
{
    public class ScriptConfiguration
    {
        private readonly string _script;
        private List<string> _namespaces;
        private List<string> _nugetDependencies;
        
        public ScriptConfiguration(string script)
        {
            _script = script;
            _namespaces = new List<string>();
            _nugetDependencies = new List<string>();
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
    }
}