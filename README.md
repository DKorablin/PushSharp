PushSharp v4.0
==============

# Development version still not finished

PushSharp is a server-side library for sending Push Notifications to iOS/OSX (APNS), Android/Chrome (GCM/FCM), Windows/Windows Phone, Amazon (ADM) and Blackberry devices!

## Sample Usage

### Firebase HTTP v1 Sample Usage

Here is how you would send a Firebase HTTP v1 notification:

```csharp
var settings = JsonConvert.DeserializeObject<FirebaseSettings>("{Firebase.ServiceAccount.json}");
var config = new FirebaseConfiguration(settings);

// Create a new broker
var broker = new FirebaseServiceBroker(config);

// Wire up events
broker.OnNotificationFailed += (notification, aggregateEx) => {

	aggregateEx.Handle (ex => {
	
		// See what kind of exception it was to further diagnose
		if (ex is GcmNotificationException notificationException) {
			
			// Deal with the failed notification
			var gcmNotification = notificationException.Notification;
			var description = notificationException.Description;

			Console.WriteLine ($"Notification Failed: ID={gcmNotification.MessageId}, Desc={description}");
		} else if (ex is GcmMulticastResultException multicastException) {

			foreach (var succeededNotification in multicastException.Succeeded) {
				Console.WriteLine ($"Notification Succeeded: ID={succeededNotification.MessageId}");
			}

			foreach (var failedKvp in multicastException.Failed) {
				var n = failedKvp.Key;
				var e = failedKvp.Value;

				Console.WriteLine ($"Notification Failed: ID={n.MessageId}, Desc={e.Description}");
			}

		} else if (ex is DeviceSubscriptionExpiredException expiredException) {
			
			var oldId = expiredException.OldSubscriptionId;
			var newId = expiredException.NewSubscriptionId;

			Console.WriteLine ($"Device RegistrationId Expired: {oldId}");

			if (!string.IsNullOrWhiteSpace (newId)) {
				// If this value isn't null, our subscription changed and we should update our database
				Console.WriteLine ($"Device RegistrationId Changed To: {newId}");
			}
		} else if (ex is RetryAfterException retryException) {
			
			// If you get rate limited, you should stop sending messages until after the RetryAfterUtc date
			Console.WriteLine ($"Rate Limited, don't send more until after {retryException.RetryAfterUtc}");
		} else {
			Console.WriteLine ("Notification Failed for some unknown reason");
		}

		// Mark it as handled
		return true;
	});
};

broker.OnNotificationSucceeded += (notification) => Console.WriteLine ("Notification Sent!");

// Start the broker
broker.Start();

foreach (var regId in MY_REGISTRATION_IDS) {
	// Queue a notification to send
	var notification = new FirebaseNotification();
	notification.message.token = regId;
	notification.message.data = JObject.Parse("{ \"somekey\" : \"somevalue\" }");

	broker.QueueNotification(notification);
}

// Stop the broker, wait for it to finish
// This isn't done after every message, but after you're
// done with the broker
broker.Stop();
```

### HuaWay Sample Usage

Here is how you would send a HuaWay notification:

```csharp
var settings = new HuaWaySettings()
{
	ApplicationId = "{ApplicationId}",
	ClientId = "{ClientId}",
	ClientSecret = "{ClientSecret}",
	ProjectId = "{ProjectId}",
};

var config = new HuaWayConfiguration(settings);
var broker = new HuaWayServiceBroker(config);
broker.OnNotificationFailed += (notification, exception) =>
{
	
};
broker.OnNotificationSucceeded += (notification) => Console.WriteLine ("Notification Sent!");

broker.Start();

foreach(var regId in Settings.Instance.HuaWayRegistrationIds)
{
	attempted++;

	var notification = new HuaWayNotification();
	notification.Message.token = new String[] { regId };
	notification.Message.data = JObject.Parse("{ \"somekey\" : \"somevalue\" }");

	broker.QueueNotification(notification);
}

broker.Stop();
```

You can read other sample usages on original author page: [Redth PushSharp](https://github.com/Redth/PushSharp)