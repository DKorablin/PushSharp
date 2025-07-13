PushSharp v4.1
==============

PushSharp is a server-side library for sending Push Notifications to iOS/OSX (APNS HTTP/2), Android/Chrome (FCM V1), HuaWay, Windows/Windows Phone, Amazon (ADM) and Blackberry devices!

## Sample Usage

### APNS HTTP/2 Sample usage

```csharp
var environment = ApnsSettings.ApnsServerEnvironment.Production;
var p8CertificatePath = "{P8CertificateFilePath}";
var p8CertificatePassword = "{P8CertificatePassword}";
var keyId = "{KeyId}";
var teamId = "{TeamId}";
var bundleId = "{com.company.appName}";

var settings = new ApnsSettings(
	environment,
	p8CertificatePath,
	p8CertificatePassword,
	keyId)
{
	AppBundleId = bundleId,
};

var config = new ApnsConfiguration(settings);
var broker = new ApnsServiceBroker(config);
broker.OnNotificationFailed += (notification, exception) =>
{

};
broker.OnNotificationSucceeded += (notification) =>
{

};

broker.Start();

foreach(var dt in Settings.Instance.ApnsDeviceTokens)
{
	broker.QueueNotification(new ApnsNotification
	{
		DeviceToken = dt,
		Payload = JObject.Parse("{ \"aps\" : { \"alert\" : \"I want cookie!\" } }")
	});
}

broker.Stop();
```

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
	notification.message.data = JObject.Parse("{ \"somekey\" : \"I want cookie\" }");

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
var clientSecret = "{ClientSecret}";
var projectId = "{ProjectId}";
var applicationId = "{ApplicationId}";

var config = new HuaWayConfiguration(clientSecret, projectId, applicationId);
var broker = new HuaWayServiceBroker(config);
broker.OnNotificationFailed += (notification, exception) =>
{
	
};
broker.OnNotificationSucceeded += (notification) => Console.WriteLine ("Notification Sent!");

broker.Start();

foreach(var regId in Settings.Instance.HuaWayRegistrationIds)
{
	var notification = new HuaWayNotification();
	notification.Message.token = new String[] { regId };
	notification.Message.data = JObject.Parse("{ \"somekey\" : \"I want cookie!\" }");

	broker.QueueNotification(notification);
}

broker.Stop();
```

You can read other sample usages on original author page: [Redth PushSharp](https://github.com/Redth/PushSharp)