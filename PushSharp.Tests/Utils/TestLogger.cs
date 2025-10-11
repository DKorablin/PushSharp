using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace AlphaOmega.PushSharp.Tests.Utils
{
	public class TestLogger : TraceListener
	{
		private readonly List<String> _messages;

		public TestLogger(List<String> messages)
			=> this._messages = messages;

		public override void Write(String message)
			=> this._messages.Add(message);

		public override void WriteLine(String message)
			=> this._messages.Add(message);
	}
}