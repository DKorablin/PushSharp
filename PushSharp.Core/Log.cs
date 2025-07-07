using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace AlphaOmega.PushSharp.Core
{
	[Flags]
	public enum LogLevel
	{
		Info = 0,
		Debug = 2,
		Error = 8
	}

	public interface ILogger
	{
		void Write(LogLevel level, String msg, params Object[] args);
	}

	public static class Log
	{
		private static readonly Object LoggerLock = new Object();

		private static readonly List<ILogger> loggers = new List<ILogger>();
		private static readonly Dictionary<CounterToken, Stopwatch> Counters = new Dictionary<CounterToken, Stopwatch>();

		static Log()
			=> AddLogger(new ConsoleLogger());

		public static void AddLogger(ILogger logger)
		{
			lock(LoggerLock)
				loggers.Add(logger);
		}

		public static void ClearLoggers()
		{
			lock(LoggerLock)
				loggers.Clear();
		}

		public static IEnumerable<ILogger> Loggers => loggers;

		public static void Write(LogLevel level, String msg, params Object[] args)
		{
			lock(loggers)
			{
				foreach(var l in loggers)
					l.Write(level, msg, args);
			}
		}

		public static void Info(String msg, params Object[] args)
			=> Write(LogLevel.Info, msg, args);

		public static void Debug(String msg, params Object[] args)
			=> Write(LogLevel.Debug, msg, args);

		public static void Error(String msg, params Object[] args)
			=> Write(LogLevel.Error, msg, args);

		public static CounterToken StartCounter()
		{
			var t = new CounterToken
			{
				Id = Guid.NewGuid().ToString()
			};

			var sw = new Stopwatch();

			Counters.Add(t, sw);

			sw.Start();

			return t;
		}

		public static TimeSpan StopCounter(CounterToken counterToken)
		{
			if(!Counters.ContainsKey(counterToken))
				return TimeSpan.Zero;

			var sw = Counters[counterToken];

			sw.Stop();

			Counters.Remove(counterToken);

			return sw.Elapsed;
		}

		public static void StopCounterAndLog(CounterToken counterToken, String msg, LogLevel level = LogLevel.Info)
		{
			var elapsed = StopCounter(counterToken);

			if(!msg.Contains("{0}"))
				msg += " {0}";

			Log.Write(level, msg, elapsed.TotalMilliseconds);
		}
	}

	public static class CounterExtensions
	{
		public static void StopAndLog(this CounterToken counterToken, String msg, LogLevel level = LogLevel.Info)
			=> Log.StopCounterAndLog(counterToken, msg, level);

		public static TimeSpan Stop(this CounterToken counterToken)
			=> Log.StopCounter(counterToken);
	}

	public class CounterToken
	{
		public String Id { get; set; }
	}

	public class ConsoleLogger : ILogger
	{
		public void Write(LogLevel level, String msg, params Object[] args)
		{
			var s = msg;

			if(args != null && args.Length > 0)
				s = String.Format(msg, args);

			var d = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.ttt");

			switch(level)
			{
			case LogLevel.Info:
				Console.Out.WriteLine(d + " [INFO] " + s);
				break;
			case LogLevel.Debug:
				Console.Out.WriteLine(d + " [DEBUG] " + s);
				break;
			case LogLevel.Error:
				Console.Error.WriteLine(d + " [ERROR] " + s);
				break;
			}
		}
	}
}