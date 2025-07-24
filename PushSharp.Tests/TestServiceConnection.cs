using System;
using System.Threading.Tasks;
using AlphaOmega.PushSharp.Core;

namespace AlphaOmega.PushSharp.Tests
{
	public class TestNotification : INotification
	{
		public static Int32 TESTID = 1;

		public TestNotification()
			=> this.TestId = TestNotification.TESTID++;

		public Object Tag { get; set; }

		public Int32 TestId { get; set; }

		public Boolean ShouldFail { get; set; }

		public override String ToString()
			=> this.TestId.ToString();

		public Boolean IsDeviceRegistrationIdValid()
			=> true;
	}

	public class TestServiceConnectionFactory : IServiceConnectionFactory<TestNotification>
	{
		public IServiceConnection<TestNotification> Create()
		{
			return new TestServiceConnection();
		}
	}

	public class TestServiceBroker : ServiceBroker<TestNotification>
	{
		public TestServiceBroker(TestServiceConnectionFactory connectionFactory) : base(connectionFactory)
		{
		}

		public TestServiceBroker() : base(new TestServiceConnectionFactory())
		{
		}
	}

	public class TestServiceConnection : IServiceConnection<TestNotification>
	{
		public async Task Send(TestNotification notification)
		{
			var id = notification.TestId;

			await Task.Delay(250).ConfigureAwait(false);

			if(notification.ShouldFail)
			{
				Console.WriteLine("Fail {0}...", id);
				throw new Exception("Notification Should Fail: " + id);
			}
		}
	}
}