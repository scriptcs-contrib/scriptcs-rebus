using System;
using Rebus;

namespace ScriptCs.Rebus
{
	public class ScriptedOwnership : IDetermineMessageOwnership
	{
		private readonly string _endpoint;

		public ScriptedOwnership(string endpoint)
		{
			_endpoint = endpoint;
		}

		public string GetEndpointFor(Type messageType)
		{
			
			return _endpoint;
		}
	}
}