using System;
using AlphaOmega.PushSharp.Core;

namespace AlphaOmega.PushSharp.HuaWay
{
	public class HuaWayException : NotificationException<HuaWayNotification>
	{
		public HuaWayResponse Response { get; private set; }

		public HuaWayException(HuaWayNotification notification, HuaWayResponse response)
			: base(response.Message, notification)
			=> this.Response = response;

		public HuaWayException(HuaWayNotification notification, System.Net.HttpStatusCode statusCode)
			: base(HuaWayException.GetStatusCodeDescription(statusCode), notification)
		{
		}

		private static String GetStatusCodeDescription(System.Net.HttpStatusCode statusCode)
		{
			switch(statusCode)
			{
			case System.Net.HttpStatusCode.BadRequest:
				return "Rectify the fault based on the status code description.";
			case System.Net.HttpStatusCode.Unauthorized:
				return "Verify the access token in the Authorization parameter in the request HTTP header.";
			case System.Net.HttpStatusCode.NotFound:
				return "Verify that the request URI is correct.";
			case System.Net.HttpStatusCode.InternalServerError:
				return "Contact Huawei technical support.";
			case System.Net.HttpStatusCode.BadGateway:
				return "Try again later or contact Huawei technical support.";
			case System.Net.HttpStatusCode.ServiceUnavailable:
				return "Set the average push speed to a value smaller than the QPS quota provided by Huawei or set the average push interval.";
			default:
				return $"Unknown status code received ({statusCode})";
			}
		}
	}
}