using System;

namespace AlphaOmega.PushSharp.Apple
{
	public class ApnsResponse
	{
		public Boolean IsSuccess { get; set; }

		public ApnsError Error { get; set; }

		public class ApnsError
		{
			/// <summaryUse <see cref="ApnsErrorReasons"/> to compare against</summary>
			public String Reason { get; set; }

			public Int64? Timestamp { get; set; }
		}
	}
}