using System;
using System.IO;
using System.Text;
using Rebus;

namespace ScriptCs.Rebus.Hosting.Extensions
{
	public class MessagingWriter : TextWriter
	{
		private readonly Action<object> _reply;

		public MessagingWriter(Action<object> reply)
		{
			if (reply == null) throw new ArgumentNullException("reply");
			_reply = reply;
		}

		public override Encoding Encoding
		{
			get { return Encoding.UTF8; }
		}

		public override void WriteLine(string value)
		{
			_reply(value);
		}

	}
}