using System;
using System.Threading.Tasks;

namespace AlphaOmega.PushSharp.Core
{
	public delegate void NotificationSuccessDelegate<in TNotification>(TNotification notification) where TNotification : INotification;
	public delegate void NotificationFailureDelegate<in TNotification>(TNotification notification, AggregateException exception) where TNotification : INotification;

	public interface IServiceConnection<in TNotification> where TNotification : INotification
	{
		Task Send(TNotification notification);
	}
}