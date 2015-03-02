using System;
using Rebus;
using ScriptCs.Contracts;

namespace ScriptCs.Rebus.Hosting.Extensions
{
	public class MessagingConsole : IConsole 
	{
		private readonly IBus _bus;

		public MessagingConsole(IBus bus)
		{
			if (bus == null) throw new ArgumentNullException("bus");
			_bus = bus;
		}

		public void Write(string value)
		{
			_bus.Reply(value);
		}

		public void WriteLine()
		{
			throw new NotImplementedException();
		}

		public void WriteLine(string value)
		{
			_bus.Reply(value);
		}

		public string ReadLine()
		{
			throw new NotImplementedException();
		}

		public void Clear()
		{
			throw new NotImplementedException();
		}

		public void Exit()
		{
			throw new NotImplementedException();
		}

		public void ResetColor()
		{
			throw new NotImplementedException();
		}

		public ConsoleColor ForegroundColor
		{
			get { throw new NotImplementedException(); }
			set { throw new NotImplementedException(); }
		}
	}
}