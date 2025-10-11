using System;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;
using System.Collections.Generic;
using System.Threading;

namespace AlphaOmega.PushSharp.Tests
{
	public class TestApnsServer
	{
		const String TAG = "TestApnsServer";

		public TestApnsServer()
			=> this.ResponseFilters = new List<ApnsResponseFilter>();

		Socket listener;
		Boolean running = false;
		Int64 totalBytesRx = 0;

		public Int32 Successful { get; set; }
		public Int32 Failed { get; set; }

		ManualResetEvent waitStop = new ManualResetEvent(false);

		public List<ApnsResponseFilter> ResponseFilters { get; set; }

#pragma warning disable 1998
		public async Task Stop()
#pragma warning restore 1998
		{
			running = false;

			try { listener.Shutdown(SocketShutdown.Both); } catch { }

			try { listener.Close(); } catch { }

			try { listener.Dispose(); } catch { }

			waitStop.WaitOne();
		}

		public async Task Start()
		{
			Console.WriteLine(TAG + " -> Starting Mock APNS Server...");
			running = true;

			listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

			listener.Bind(new IPEndPoint(IPAddress.Any, 2195));

			listener.Listen(100);

			Console.WriteLine(TAG + " -> Started Mock APNS Server.");

			while(running)
			{

				Socket socket = null;

				try
				{
					// Get a client connection
					socket = await Task.Factory.FromAsync<Socket>(
						listener.BeginAccept(null, null),
						listener.EndAccept).ConfigureAwait(false);
				} catch
				{
				}

				if(socket == null)
					break;

				Console.WriteLine(TAG + " -> Client Connected.");


				// Start receiving from the client connection on a new thread
#pragma warning disable 4014
				Task.Factory.StartNew(() =>
				{
#pragma warning restore 4014

					var sentErrorResponse = false;
					var s = socket;
					Byte[] buffer = new Byte[1024000]; // 1 MB

					var data = new List<Byte>();

					// Do processing, continually receiving from the socket 
					while(true)
					{
						var received = s.Receive(buffer);

						if(received <= 0 && data.Count <= 0)
							break;

						totalBytesRx += received;

						Console.WriteLine(TAG + " -> Received {0} bytes...", received);

						// Add the received data to our data list
						for(Int32 i = 0; i < received; i++)
							data.Add(buffer[i]);

						ApnsServerNotification notification = null;

						try
						{

							while((notification = Parse(data)) != null)
							{

								if(!sentErrorResponse)
									Successful++;

								// Console.WriteLine (TAG + " -> Rx'd ID: {0}, DeviceToken: {1}, Payload: {2}", notification.Identifier, notification.DeviceToken, notification.Payload);

								foreach(var rf in ResponseFilters)
								{
									if(rf.IsMatch(notification.Identifier, notification.DeviceToken, notification.Payload))
									{
										if(!sentErrorResponse)
											SendErrorResponse(s, rf.Status, notification.Identifier);
										sentErrorResponse = true;
										break;
									}
								}
							}


						} catch(ApnsNotificationException ex)
						{

							Console.WriteLine(TAG + " -> Notification Exception: {0}", ex);

							if(!sentErrorResponse)
								SendErrorResponse(s, ex.ErrorStatusCode, ex.NotificationId);
							sentErrorResponse = true;

							break;
						}

					}

					try
					{
						s.Shutdown(SocketShutdown.Both);
					} catch
					{
					}
					try
					{
						s.Close();
					} catch
					{
					}
					try
					{
						s.Dispose();
					} catch
					{
					}

					Console.WriteLine(TAG + " -> Client Disconnected...");
				});
			}

			waitStop.Set();

			Console.WriteLine(TAG + " -> Stopped APNS Server.");
		}


		void SendErrorResponse(Socket s, ApnsNotificationErrorStatusCode statusCode, Int32 identifier)
		{
			Failed++;
			Successful--;

			var errorResponseData = new Byte[6];
			errorResponseData[0] = 0x01;
			errorResponseData[1] = BitConverter.GetBytes((Int16)statusCode)[0];

			var id = BitConverter.GetBytes(IPAddress.HostToNetworkOrder(identifier));
			Buffer.BlockCopy(id, 0, errorResponseData, 2, 4);

			var sent = Task.Factory.FromAsync<Int32>(
				s.BeginSend(errorResponseData, 0, errorResponseData.Length, SocketFlags.None, null, null),
				s.EndSend).Result;
		}

		ApnsServerNotification Parse(List<Byte> data)
		{
			// COMMAND          FRAME LENGTH    FRAME
			// 1 byte (0x02)    4 bytes         ITEM ... ITEM ... ITEM ...

			// ITEM ID          ITEM LENGTH     ITEM DATA
			// 1 byte           2 bytes         variable length

			// ITEMS:
			// 1: 32 bytes                      Device Token
			// 2: 2048 bytes (up to)            Payload
			// 3: 4 bytes                       Notification identifier
			// 4: 4 bytes                       Expiration
			// 5: 1 byte                        Priority (10 - immediate, or 5 - normal)

			var notification = new ApnsServerNotification();

			ApnsNotificationException exception = null;

			// If there aren't even 5 bytes, we can't even check if the length of notification is correct
			if(data.Count < 5)
				return null;

			// Let's check to see if the notification is all here in the buffer
			var apnsCmd = data[0];
			var apnsFrameLength = IPAddress.NetworkToHostOrder(BitConverter.ToInt32(data.GetRange(1, 4).ToArray(), 0));

			// If we don't have all of the notification's frame data that we should have, we need to keep waiting
			if(data.Count - 5 < apnsFrameLength)
				return null;

			var frameData = data.GetRange(5, apnsFrameLength);

			// Remove the data we are processing
			data.RemoveRange(0, apnsFrameLength + 5);

			// Now process each item from the frame
			while(frameData.Count > 0)
			{

				// We need at least 4 bytes to count as a full item (1 byte id + 2 bytes length + at least 1 byte data)
				if(frameData.Count < 4)
				{
					exception = new ApnsNotificationException(ApnsNotificationErrorStatusCode.ProcessingError, "Invalid Frame Data");
					break;
				}

				var itemId = frameData[0];
				var itemLength = IPAddress.NetworkToHostOrder(BitConverter.ToInt16(frameData.GetRange(1, 2).ToArray(), 0));

				// Make sure the item data is all there
				if(frameData.Count - 3 < itemLength)
				{
					exception = new ApnsNotificationException(ApnsNotificationErrorStatusCode.ProcessingError, "Invalid Item Data");
					break;
				}

				var itemData = frameData.GetRange(3, itemLength);
				frameData.RemoveRange(0, itemLength + 3);

				if(itemId == 1)
				{ // Device Token

					notification.DeviceToken = BitConverter.ToString(itemData.ToArray()).Replace("-", "");
					if(notification.DeviceToken.Length != 64)
						exception = new ApnsNotificationException(ApnsNotificationErrorStatusCode.InvalidTokenSize, "Invalid Token Size");

				} else if(itemId == 2)
				{ // Payload

					notification.Payload = BitConverter.ToString(itemData.ToArray());
					if(notification.Payload.Length > 2048)
						exception = new ApnsNotificationException(ApnsNotificationErrorStatusCode.InvalidPayloadSize, "Invalid Payload Size");

				} else if(itemId == 3)
				{ // Identifier

					if(itemData.Count > 4)
						exception = new ApnsNotificationException(ApnsNotificationErrorStatusCode.ProcessingError, "Identifier too long");
					else
						notification.Identifier = IPAddress.NetworkToHostOrder(BitConverter.ToInt32(itemData.ToArray(), 0));

				} else if(itemId == 4)
				{ // Expiration

					Int32 secondsSinceEpoch = IPAddress.NetworkToHostOrder(BitConverter.ToInt32(itemData.ToArray(), 0));

					var expire = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddSeconds(secondsSinceEpoch);
					notification.Expiration = expire;

				} else if(itemId == 5)
				{ // Priority
					notification.Priority = itemData[0];
				}
			}

			if(exception == null && String.IsNullOrEmpty(notification.DeviceToken))
				exception = new ApnsNotificationException(ApnsNotificationErrorStatusCode.MissingDeviceToken, "Missing Device Token");
			if(exception == null && String.IsNullOrEmpty(notification.Payload))
				exception = new ApnsNotificationException(ApnsNotificationErrorStatusCode.MissingPayload, "Missing Payload");

			// See if there was an error and we can assign an ID to it
			if(exception != null)
			{
				exception.NotificationId = notification.Identifier;

				throw exception;
			}

			return notification;
		}
	}

	public class ApnsServerNotification
	{
		public String DeviceToken { get; set; }
		public String Payload { get; set; }
		public Int32 Identifier { get; set; }
		public DateTime? Expiration { get; set; }
		public Int16 Priority { get; set; }
	}

	public enum ApnsNotificationErrorStatusCode
	{
		NoErrors = 0,
		ProcessingError = 1,
		MissingDeviceToken = 2,
		MissingTopic = 3,
		MissingPayload = 4,
		InvalidTokenSize = 5,
		InvalidTopicSize = 6,
		InvalidPayloadSize = 7,
		InvalidToken = 8,
		Shutdown = 10,
		Unknown = 255
	}

	public class ApnsNotificationException : Exception
	{
		public ApnsNotificationException() : base()
		{
		}

		public ApnsNotificationException(ApnsNotificationErrorStatusCode statusCode, String msg) : base(msg)
			=> this.ErrorStatusCode = statusCode;

		public Int32 NotificationId { get; set; }
		public ApnsNotificationErrorStatusCode ErrorStatusCode { get; set; }
	}

	public class ApnsResponseFilter
	{
		public ApnsResponseFilter(IsMatchDelegate isMatchHandler) : this(ApnsNotificationErrorStatusCode.ProcessingError, isMatchHandler)
		{
		}

		public ApnsResponseFilter(ApnsNotificationErrorStatusCode status, IsMatchDelegate isMatchHandler)
		{
			this.IsMatch = isMatchHandler;
			this.Status = status;
		}

		public delegate Boolean IsMatchDelegate(Int32 identifier, String deviceToken, String payload);

		public IsMatchDelegate IsMatch { get; set; }

		public ApnsNotificationErrorStatusCode Status { get; set; }
	}
}