namespace AlphaOmega.PushSharp.Core
{
	/// <summary>The push notification connection factory.</summary>
	public interface IServiceConnectionFactory<in TNotification> where TNotification : INotification
	{
		/// <summary>Create instance if connection factory for notification distribution.</summary>
		/// <returns>The push service connection instance.</returns>
		IServiceConnection<TNotification> Create();
	}
}