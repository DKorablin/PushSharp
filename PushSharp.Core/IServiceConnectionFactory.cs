namespace AlphaOmega.PushSharp.Core
{
	public interface IServiceConnectionFactory<in TNotification> where TNotification : INotification
	{
		IServiceConnection<TNotification> Create();
	}
}