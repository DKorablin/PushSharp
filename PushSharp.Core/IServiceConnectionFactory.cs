namespace AlphaOmega.PushSharp.Core
{
	public interface IServiceConnectionFactory<TNotification> where TNotification : INotification
	{
		IServiceConnection<TNotification> Create();
	}
}