using System;
using System.Threading.Tasks;

namespace AlphaOmega.PushSharp.Core
{
	/// <summary>The delegate that is usen to notify subscribers when notification message sent successfully.</summary>
	/// <typeparam name="TNotification">The type of notification.</typeparam>
	/// <param name="notification">The notification that was successfully sent.</param>
	public delegate void NotificationSuccessDelegate<in TNotification>(TNotification notification) where TNotification : INotification;

	/// <summary>The delegate that is used to notify subscribers when we failed to send push notification.</summary>
	/// <typeparam name="TNotification">The type of notification.</typeparam>
	/// <param name="notification">The notification instance that we failed to send.</param>
	/// <param name="exception">The reason whe we can't send notification.</param>
	public delegate void NotificationFailureDelegate<in TNotification>(TNotification notification, AggregateException exception) where TNotification : INotification;

	/// <summary>The service connection interface that is responsible for notification processing between client and push servers.</summary>
	/// <typeparam name="TNotification">The type of notification to send.</typeparam>
	public interface IServiceConnection<in TNotification> where TNotification : INotification
	{
		/// <summary>Send notification message to push server.</summary>
		/// <param name="notification">The notification message instance.</param>
		/// <returns>The async operation result.</returns>
		Task Send(TNotification notification);
	}
}