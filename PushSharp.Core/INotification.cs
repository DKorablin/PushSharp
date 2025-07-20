using System;

namespace AlphaOmega.PushSharp.Core
{
	/// <summary>The base interface for PUSH notifications implementations.</summary>
	public interface INotification
	{
		/// <summary>Validates that notification message is valid before sending it to PUSH server.</summary>
		/// <returns></returns>
		Boolean IsDeviceRegistrationIdValid();

		/// <summary>The extra object associated with current notification instance.</summary>
		Object Tag { get; set; }
	}
}