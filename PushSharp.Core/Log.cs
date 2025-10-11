using System;
using System.Diagnostics;

namespace AlphaOmega.PushSharp.Core
{
	/// <summary>The logic to log all messages to the different data sources.</summary>
	public static class Log
	{
		/// <summary>The trace source instance to log all events.</summary>
		public static TraceSource Trace { get; } = new TraceSource("AlphaOmega.PushSharp");

		/// <summary>Adds trace listener to the trace source and remove Default trace listener.</summary>
		/// <remarks>Useful when impossible to add listener using config file.</remarks>
		/// <param name="listener">The trace listener to add.</param>
		/// <exception cref="ArgumentNullException"><paramref name="listener"/> is required argument.</exception>
		public static void AddTraceListener(TraceListener listener)
		{
			_ = listener ?? throw new ArgumentNullException(nameof(listener));

			Log.Trace.Listeners.Remove("Default");
			Log.Trace.Switch.Level = SourceLevels.All;
			Log.Trace.Listeners.Add(listener);
		}
	}
}