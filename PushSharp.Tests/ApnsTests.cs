using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using AlphaOmega.PushSharp.Apple;
using AlphaOmega.PushSharp.Core;
using Xunit;

namespace AlphaOmega.PushSharp.Tests
{
	[Collection("Fake")]
	public class ApnsTests
	{
		[Fact]
		public async Task APNS_Single_Succeeds()
		{
			await Apns(0, 1, new List<ApnsResponseFilter>());
		}

		[Fact]
		public async Task APNS_Multiple_Succeed()
		{
			await Apns(0, 3, new List<ApnsResponseFilter>());
		}

		[Fact(Skip = Settings.REMOVED)]
		public async Task APNS_Send_Many()
		{
			await Apns(10, 1010, new List<ApnsResponseFilter> {
				new ApnsResponseFilter ((id, deviceToken, payload) => {
					return id % 100 == 0;
				})
			});
		}

		[Fact]
		public async Task APNS_Send_Small()
		{
			await Apns(2, 256, new List<ApnsResponseFilter>() {
				new ApnsResponseFilter ((id, deviceToken, payload) => {
					return id % 100 == 0;
				})
			});
		}

		[Fact(Skip = Settings.REMOVED)]
		public async Task APNS_Scale_Brokers()
		{
			await Apns(10, 10100, new List<ApnsResponseFilter> {
				new ApnsResponseFilter ((id, deviceToken, payload) => {
					return id % 1000 == 0;
				})
			}, scale: 2);
		}

		[Fact]
		public async Task Should_Fail_Connect()
		{
			Int32 count = 2;
			Int64 failed = 0;
			Int64 success = 0;

			var server = new TestApnsServer();
#pragma warning disable 4014
			server.Start();
#pragma warning restore 4014

			var config = new ApnsConfiguration("invalidhost", 2195);
			var broker = new ApnsServiceBroker(config);
			broker.OnNotificationFailed += (notification, exception) =>
			{
				Interlocked.Increment(ref failed);
				Console.WriteLine("Failed: " + notification.Identifier);
			};
			broker.OnNotificationSucceeded += (notification) => Interlocked.Increment(ref success);
			broker.Start();

			for(int i = 0; i < count; i++)
			{
				broker.QueueNotification(new ApnsNotification
				{
					DeviceToken = (i + 1).ToString().PadLeft(64, '0'),
					Payload = JObject.Parse(@"{""aps"":{""badge"":" + (i + 1) + "}}")
				});
			}

			broker.Stop();
			await server.Stop().ConfigureAwait(false);

			var actualFailed = failed;
			var actualSuccess = success;

			Console.WriteLine("Success: {0}, Failed: {1}", actualSuccess, actualFailed);

			Assert.Equal(count, actualFailed);//, "Expected Failed Count not met");
			Assert.Equal(0, actualSuccess);//, "Expected Success Count not met");
		}

		private async Task Apns(Int32 expectFailed, Int32 numberNotifications, IEnumerable<ApnsResponseFilter> responseFilters, Int32 batchSize = 1000, Int32 scale = 1)
		{
			var notifications = new List<ApnsNotification>();

			for(Int32 i = 0; i < numberNotifications; i++)
			{
				notifications.Add(new ApnsNotification
				{
					DeviceToken = (i + 1).ToString().PadLeft(64, '0'),
					Payload = JObject.Parse(@"{""aps"":{""badge"":" + (i + 1) + "}}")
				});
			}

			await Apns(expectFailed, notifications, responseFilters, batchSize, scale).ConfigureAwait(false);
		}

		private async Task Apns(Int32 expectFailed, List<ApnsNotification> notifications, IEnumerable<ApnsResponseFilter> responseFilters, Int32 batchSize = 1000, Int32 scale = 1)
		{
			Int64 success = 0;
			Int64 failed = 0;

			var server = new TestApnsServer();
			server.ResponseFilters.AddRange(responseFilters);

			// We don't want to await this, so we can start the server and listen without blocking
#pragma warning disable 4014
			server.Start();
#pragma warning restore 4014

			var config = new ApnsConfiguration("127.0.0.1", 2195)
			{
				InternalBatchSize = batchSize
			};


			var broker = new ApnsServiceBroker(config);
			broker.OnNotificationFailed += (notification, exception) => Interlocked.Increment(ref failed);
			broker.OnNotificationSucceeded += (notification) => Interlocked.Increment(ref success);

			broker.Start();

			if(scale != 1)
				broker.ChangeScale(scale);

			var c = Log.StartCounter();

			foreach(var n in notifications)
				broker.QueueNotification(n);

			broker.Stop();

			c.StopAndLog("Test Took {0} ms");

			await server.Stop().ConfigureAwait(false);

			var expectedSuccess = notifications.Count - expectFailed;

			var actualFailed = failed;
			var actualSuccess = success;

			Console.WriteLine("EXPECT: Successful: {0}, Failed: {1}", expectedSuccess, expectFailed);
			Console.WriteLine("SERVER: Successful: {0}, Failed: {1}", server.Successful, server.Failed);
			Console.WriteLine("CLIENT: Successful: {0}, Failed: {1}", actualSuccess, actualFailed);

			Assert.Equal(expectFailed, actualFailed);
			Assert.Equal(expectedSuccess, actualSuccess);

			Assert.Equal(server.Failed, actualFailed);
			Assert.Equal(server.Successful, actualSuccess);
		}
	}
}