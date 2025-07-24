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

		public override void Write(String o)
			=> this._messages.Add(o);

		public override void WriteLine(String o)
			=> this._messages.Add(o);
	}
}