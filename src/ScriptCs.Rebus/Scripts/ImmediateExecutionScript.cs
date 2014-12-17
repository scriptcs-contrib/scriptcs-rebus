namespace ScriptCs.Rebus.Scripts
{
    public class ImmediateExecutionScript
    {
        public string ScriptContent { get; set; }
        public bool UseMono { get; set; }
        public string[] NuGetDependencies { get; set; }
        public string[] Namespaces { get; set; }
        public string[] LocalDependencies { get; set; }
        public bool UseLogging { get; set; }
    }
}