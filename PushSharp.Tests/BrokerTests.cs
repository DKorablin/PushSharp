using System.Linq;
using System.Threading.Tasks;
using PushSharp.Core;
using Xunit;


namespace PushSharp.Tests
{
	[Collection ("Core")]
	public class BrokerTests
	{
		[Fact]
        public void Broker_Send_Many ()
		{
            var succeeded = 0;
            var failed = 0;
            var attempted = 0;

            var broker = new TestServiceBroker ();
            broker.OnNotificationFailed += (notification, exception) => {
                failed++;
            };
            broker.OnNotificationSucceeded += (notification) => {
                succeeded++;  
            };
			broker.Start ();
			broker.ChangeScale (1);

            var c = Log.StartCounter ();

            for (int i = 1; i <= 1000; i++) {
                attempted++;
                broker.QueueNotification (new TestNotification { TestId = i });
            }

			broker.Stop ();
		
            c.StopAndLog ("Test Took {0} ms");

            Assert.Equal (attempted, succeeded);
            Assert.Equal (0, failed);
		}


        [Fact]
        #pragma warning disable 1998
        public async Task Broker_Some_Fail ()
        #pragma warning restore 1998
        {
            var succeeded = 0;
            var failed = 0;
            var attempted = 0;

            const int count = 10;
            var failIds = new [] { 3, 5, 7 };

            var broker = new TestServiceBroker ();
            broker.OnNotificationFailed += (notification, exception) =>
                failed++;
            broker.OnNotificationSucceeded += (notification) =>
                succeeded++;

            broker.Start ();
            broker.ChangeScale (1);

            var c = Log.StartCounter ();

            for (int i = 1; i <= count; i++) {
                attempted++;
                broker.QueueNotification (new TestNotification {
                    TestId = i,
                    ShouldFail = failIds.Contains (i)
                });
            }

            broker.Stop ();

            c.StopAndLog ("Test Took {0} ms");

            Assert.Equal (attempted - failIds.Length, succeeded);
            Assert.Equal (failIds.Length, failed);
        }
    }
}