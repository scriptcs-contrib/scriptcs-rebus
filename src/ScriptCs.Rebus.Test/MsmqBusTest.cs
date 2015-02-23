using NUnit.Framework;
using Rebus.Configuration;

namespace ScriptCs.Rebus.Test
{
	public class MsmqBusTest
	{
		[TestFixture]
		internal class SendMethod : TestBase<MsmqBus>
		{
			///<summary>
			/// Verify that Send() is called upon BaseBus.
			///</summary>
			[Test]
			public void SendIsCalled()
			{
				// Arrange
				
				
				// Act

				// Assert
			}
			
		}
 
	}
}