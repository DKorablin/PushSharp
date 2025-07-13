# PushSharp v4.1
[![Auto build](https://github.com/DKorablin/PushSharp/actions/workflows/release.yml/badge.svg)](https://github.com/DKorablin/PushSharp/releases/latest)
[![Nuget](https://img.shields.io/nuget/v/AlphaOmega.PushSharp)](https://www.nuget.org/packages/AlphaOmega.PushSharp)

PushSharp is a server-side library for sending Push Notifications to iOS/OSX (APNS HTTP/2), Android/Chrome (FCM V1), Huawei (HMS), Windows/Windows Phone, Amazon (ADM) and Blackberry devices!

Improvements:
1. Added Apple APNS HTTP/2 support (New version)
2. Added Firebase Cloud Messaging V1 support (New version)
3. Added Huawei HMS V1 (New) [V2 in progress...]
4. Changed assembly signing key file for all assemblies. (PublicKeyToken=a8ac5fc45c3adb8d)
5. Added PE file signing. (S/N: 00c18bc05b61a77408c694bd3542d035)
6. Added CI/CD pipelines

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

foreach(var dt in MY_DEVICE_TOKEN_IDS)
{
	broker.QueueNotification(new ApnsNotification
	{
		DeviceToken = dt,
		Payload = JObject.Parse("{ \"aps\" : { \"alert\" : \"I want cookie\" } }")
	});
}

broker.Stop();
```

### Firebase HTTP v1 Sample Usage

Here is how you would send a Firebase HTTP v1 notification:

```csharp
var projectId = "{ProjectId}";
var privateKey = "{PrivateKey}";
var clientEmail = "{ClientEmail}";
var tokenUri = "{TokenUri}";

var settings = new FirebaseSettings(projectId, privateKey, clientEmail, tokenUri);
// var settings = JsonConvert.DeserializeObject<FirebaseSettings>("{Firebase.ServiceAccount.json}");
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

### Huawei (HMS) Sample Usage

Here is how you would send a Huawei (HMS) notification:

```csharp
var clientSecret = "{ClientSecret}";
var projectId = "{ProjectId}";
var applicationId = "{ApplicationId}";

var config = new HuaweiConfiguration(clientSecret, projectId, applicationId);
var broker = new HuaweiServiceBroker(config);

broker.OnNotificationFailed += (notification, exception) =>
{
	
};

broker.OnNotificationSucceeded += (notification) => Console.WriteLine ("Notification Sent!");

broker.Start();

foreach(var regId in MY_REGISTRATION_IDS)
{
	var notification = new HuaweiNotification();
	notification.Message.token = new String[] { regId };
	notification.Message.data = JObject.Parse("{ \"somekey\" : \"I want cookie\" }");

	broker.QueueNotification(notification);
}

broker.Stop();
```

You can read other sample usages on original author page: [Redth PushSharp](https://github.com/Redth/PushSharp)