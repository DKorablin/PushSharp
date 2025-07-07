using System;

namespace AlphaOmega.PushSharp.HuaWay
{
	public class HuaWaySettings
	{
		public String ClientId { get; set; }

		public String ClientSecret { get; set; }

		/// <summary>https://push-api.cloud.huawei.com/v2/projectid/messages:send</summary>
		public String ProjectId { get; set; }

		/// <summary>https://push-api.cloud.huawei.com/v1/appid/messages:send</summary>
		public String ApplicationId { get; set; }
	}
}