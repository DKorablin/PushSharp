PushSharp v4.0
==============

PushSharp is a server-side library for sending Push Notifications to iOS/OSX (APNS), Android/Chrome (GCM/FCM), Windows/Windows Phone, Amazon (ADM) and Blackberry devices!

## Sample Usage

### GCM/FCM Sample Usage

Here is how you would send a Firebase HTTP v1 notification:

```csharp
// Configuration GCM (use this section for GCM)
var config = new GcmConfiguration ("GCM-SENDER-ID", "AUTH-TOKEN", null);
var provider = "GCM";

// Configuration FCM (use this section for FCM)
// var config = new GcmConfiguration("APIKEY");
// config.GcmUrl = "https://fcm.googleapis.com/fcm/send";
// var provider = "FCM";

// Create a new broker
var gcmBroker = new GcmServiceBroker (config);
    
// Wire up events
gcmBroker.OnNotificationFailed += (notification, aggregateEx) => {

	aggregateEx.Handle (ex => {
	
		// See what kind of exception it was to further diagnose
		if (ex is GcmNotificationException notificationException) {
			
			// Deal with the failed notification
			var gcmNotification = notificationException.Notification;
			var description = notificationException.Description;

			Console.WriteLine ($"{provider} Notification Failed: ID={gcmNotification.MessageId}, Desc={description}");
		} else if (ex is GcmMulticastResultException multicastException) {

			foreach (var succeededNotification in multicastException.Succeeded) {
				Console.WriteLine ($"{provider} Notification Succeeded: ID={succeededNotification.MessageId}");
			}

			foreach (var failedKvp in multicastException.Failed) {
				var n = failedKvp.Key;
				var e = failedKvp.Value;

				Console.WriteLine ($"{provider} Notification Failed: ID={n.MessageId}, Desc={e.Description}");
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
			Console.WriteLine ($"{provider} Rate Limited, don't send more until after {retryException.RetryAfterUtc}");
		} else {
			Console.WriteLine ("{provider} Notification Failed for some unknown reason");
		}

		// Mark it as handled
		return true;
	});
};

gcmBroker.OnNotificationSucceeded += (notification) => {
	Console.WriteLine ("{provider} Notification Sent!");
};

// Start the broker
gcmBroker.Start ();

foreach (var regId in MY_REGISTRATION_IDS) {
	// Queue a notification to send
	gcmBroker.QueueNotification (new GcmNotification {
		RegistrationIds = new List<string> { 
			regId
		},
		Data = JObject.Parse ("{ \"somekey\" : \"somevalue\" }")
	});
}
   
// Stop the broker, wait for it to finish   
// This isn't done after every message, but after you're
// done with the broker
gcmBroker.Stop ();
```

You can read other sample usages on original author page: [Redth PushSharp](https://github.com/Redth/PushSharp)