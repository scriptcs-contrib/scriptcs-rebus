using System;
using System.IO;
using Rebus;
using ScriptCs.Contracts;

namespace ScriptCs.Rebus.Hosting.Extensions
{
	public class MessagingConsole : IConsole
	{
		private readonly Action<object> _reply;

		public MessagingConsole(Action<object> reply)
		{
			if (reply == null) throw new ArgumentNullException("reply");
			_reply = reply;

			Console.SetOut(new MessagingWriter(reply));
		}

		public void Write(string value)
		{
			throw new NotImplementedException();
		}

		public void WriteLine()
		{
		}

		public void WriteLine(string value)
		{
			_reply(value);
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
			get { return Console.ForegroundColor; }
			set { Console.ForegroundColor = value; }
		}
	}
}