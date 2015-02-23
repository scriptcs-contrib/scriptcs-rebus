using System;
using System.IO;
using ScriptCs.Contracts;

namespace ScriptCs.Rebus.Hosting.ScriptHandlers.WebApi
{
	public class WebApiScriptHack : IScriptPack
	{
		public void Initialize(IScriptPackSession session)
		{
			session.AddReference(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "bin", "System.Web.Http.dll"));
			session.AddReference("System.Net.Http");
			session.ImportNamespace("System.Web.Http");
			session.ImportNamespace("System.Net.Http");
		}

		public IScriptPackContext GetContext()
		{
			return null;
		}

		public void Terminate()
		{
		}
	}
}