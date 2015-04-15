using System;
using ScriptCs.Contracts;

namespace ScriptCs.Rebus
{
    public class RebusScriptBus : IScriptPackContext
    {
	    private MsmqBus _msmqBus;

	    public BaseBus ConfigureBus(string endpoint)
        {
	        _msmqBus = new MsmqBus(endpoint, this);
	        return _msmqBus;
        }

		//public void Dispose()
		//{
		//	this.Dispose(true);
		//	GC.SuppressFinalize(this);
		//}

		//protected virtual void Dispose(bool disposing)
		//{
		//	if (disposing)
		//	{
		//		//this.ShutDown();
		//	}
		//}

		//private void ShutDown()
		//{
		//	_msmqBus.ShutDown();
		//}
    }

}
