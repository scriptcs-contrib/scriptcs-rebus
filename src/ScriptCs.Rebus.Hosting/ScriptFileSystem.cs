using System;

namespace ScriptCs.Rebus.Hosting
{
	/// <summary>
	/// By default, the scriptcs.hosting inspects the bin folder for references. This class overrides the filesystem, by setting the same folder as the executing assembly.
	/// </summary>
	internal class ScriptFileSystem : FileSystem
	{
		public override string BinFolder
		{
			get { return string.Empty; }
		}
	}
}