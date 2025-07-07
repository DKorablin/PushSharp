using System;

namespace AlphaOmega.PushSharp.Core
{
	public interface INotification
	{
		Boolean IsDeviceRegistrationIdValid();
		Object Tag { get; set; }
	}
}