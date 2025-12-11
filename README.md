# PushSharp v4.1

[![Auto build](https://github.com/DKorablin/PushSharp/actions/workflows/release.yml/badge.svg)](https://github.com/DKorablin/PushSharp/releases/latest)
[![Nuget](https://img.shields.io/nuget/v/AlphaOmega.PushSharp)](https://www.nuget.org/packages/AlphaOmega.PushSharp)

A server-side .NET library for sending Push Notifications to iOS/OSX (APNS HTTP/2), Android/Chrome (FCM V1), Huawei (HMS), Windows/Windows Phone, Amazon (ADM), and Blackberry devices.

## Features

- **Apple Push Notification Service (APNS)** - HTTP/2 based with token authentication
- **Firebase Cloud Messaging (FCM)** - V1 API support with OAuth 2.0
- **Huawei Mobile Services (HMS)** - Push Kit V1/V2 support
- **Amazon Device Messaging (ADM)** - For Amazon Fire devices
- **Windows Push Notification Services (WNS)** - For Windows/Windows Phone
- **Blackberry** - Push notification support

## What's New in v4.1

1. ✅ Added Apple APNS HTTP/2 support (replacing legacy binary protocol)
2. ✅ Added Firebase Cloud Messaging V1 support (OAuth 2.0 based)
3. ✅ Added Huawei HMS V1 support (V2 in progress)
4. ✅ Updated .NET Framework from v4.5 to v4.8
5. ✅ Added .NET Standard v2.0 support
6. ✅ Changed assembly signing key for all assemblies (PublicKeyToken=a8ac5fc45c3adb8d)
7. ✅ Added PE file signing (S/N: 00c18bc05b61a77408c694bd3542d035)
8. ✅ Added CI/CD pipelines

## Installation

Install via NuGet Package Manager:

```bash
dotnet add package AlphaOmega.PushSharp
```

Or via Package Manager Console:

```powershell
Install-Package AlphaOmega.PushSharp
```

## Requirements

- .NET Framework 4.8 or higher
- .NET Standard 2.0 compatible runtime (.NET Core 2.0+, .NET 5+, .NET 6+, etc.)

## Sample Usage

### APNS HTTP/2 (Apple Push Notification Service)

Send notifications to iOS and macOS devices using Apple's modern HTTP/2 based API:

```csharp
using AlphaOmega.PushSharp.Apple;
using Newtonsoft.Json.Linq;

var environment = ApnsSettings.ApnsServerEnvironment.Production;
var keyId = "{Your-Key-ID}";           // 10-character Key ID from Apple Developer Portal
var teamId = "{Your-Team-ID}";         // Your Apple Developer Team ID
var bundleId = "com.company.appName";  // Your app's bundle identifier

// Create APNS settings
var settings = new ApnsSettings(environment, keyId, teamId, bundleId);
settings.LoadP8CertificateFromFile("{Path-To-P8-Certificate}");

// Configure and create broker
var config = new ApnsConfiguration(settings);
var broker = new ApnsServiceBroker(config);

// Wire up event handlers
broker.OnNotificationFailed += (notification, exception) =>
{
	Console.WriteLine($"Notification failed: {exception.Message}");
};

broker.OnNotificationSucceeded += (notification) =>
{
	Console.WriteLine("Notification sent successfully!");
};

// Start the broker
broker.Start();

// Queue notifications
foreach (var deviceToken in MY_DEVICE_TOKEN_IDS)
{
	broker.QueueNotification(new ApnsNotification
	{
		DeviceToken = deviceToken,
		Payload = JObject.Parse("{ \"aps\" : { \"alert\" : \"Hello from PushSharp!\" } }")
	});
}

// Stop the broker when done
broker.Stop();
```

#### APNS Configuration Notes

- **P8 Certificate**: Download your .p8 authentication key from Apple Developer Portal
- **Key ID**: Found in the Apple Developer Portal when you create the key
- **Team ID**: Located in your Apple Developer account membership details
- **Bundle ID**: Your app's unique identifier (e.g., com.company.appname)
- **Environment**: Use `Development` for sandbox testing, `Production` for live apps

### Firebase Cloud Messaging V1 (FCM)

Send notifications to Android and Chrome devices using Firebase:

```csharp
using AlphaOmega.PushSharp.Google;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

// Option 1: Manual settings
var projectId = "{Your-Project-ID}";
var privateKey = "{Your-Private-Key}";
var clientEmail = "{Your-Client-Email}";
var tokenUri = "https://oauth2.googleapis.com/token";

var settings = new FirebaseSettings(projectId, privateKey, clientEmail, tokenUri);

// Option 2: Load from Firebase service account JSON file
// var settings = JsonConvert.DeserializeObject<FirebaseSettings>(
//	File.ReadAllText("Firebase.ServiceAccount.json"));

var config = new FirebaseConfiguration(settings);
var broker = new FirebaseServiceBroker(config);

// Wire up event handlers
broker.OnNotificationFailed += (notification, aggregateEx) =>
{
	aggregateEx.Handle(ex =>
	{
		if (ex is GcmNotificationException notificationException)
		{
			var gcmNotification = notificationException.Notification;
			var description = notificationException.Description;
			Console.WriteLine($"Notification Failed: ID={gcmNotification.MessageId}, Desc={description}");
		}
		else if (ex is GcmMulticastResultException multicastException)
		{
			foreach (var succeededNotification in multicastException.Succeeded)
				Console.WriteLine($"Notification Succeeded: ID={succeededNotification.MessageId}");

			foreach (var failedKvp in multicastException.Failed)
			{
				var n = failedKvp.Key;
				var e = failedKvp.Value;
				Console.WriteLine($"Notification Failed: ID={n.MessageId}, Desc={e.Description}");
			}
		}
		else if (ex is DeviceSubscriptionExpiredException expiredException)
		{
			var oldId = expiredException.OldSubscriptionId;
			Console.WriteLine($"Device RegistrationId Expired: {oldId}");
		}
		else if (ex is RetryAfterException retryException)
		{
			Console.WriteLine($"Rate Limited, retry after {retryException.RetryAfterUtc}");
		}
		else
		{
			Console.WriteLine("Notification Failed for unknown reason");
		}

		return true; // Mark as handled
	});
};

broker.OnNotificationSucceeded += (notification) =>
{
	Console.WriteLine("Notification Sent!");
};

// Start the broker
broker.Start();

// Queue notifications
foreach (var regId in MY_REGISTRATION_IDS)
{
	var notification = new FirebaseNotification();
	notification.message.token = regId;
	notification.message.data = JObject.Parse("{ \"somekey\" : \"Hello from PushSharp!\" }");

	broker.QueueNotification(notification);
}

// Stop the broker when done
broker.Stop();
```

#### Firebase Configuration Notes

- **Service Account JSON**: Download from Firebase Console → Project Settings → Service Accounts
- **Project ID**: Found in the Firebase Console project settings
- **Private Key**: Included in the service account JSON file
- **Client Email**: The service account email address

### Huawei Mobile Services (HMS)

Send notifications to Huawei devices:

```csharp
using AlphaOmega.PushSharp.Huawei;
using Newtonsoft.Json.Linq;

var clientSecret = "{Your-Client-Secret}";
var projectId = "{Your-Project-ID}";        // For V2 API
var applicationId = "{Your-Application-ID}"; // For V1 API (legacy)

// Use either projectId (V2) OR applicationId (V1), not both
var config = new HuaweiConfiguration(clientSecret, null, applicationId);
var broker = new HuaweiServiceBroker(config);

// Wire up event handlers
broker.OnNotificationFailed += (notification, exception) =>
{
	Console.WriteLine($"Notification failed: {exception.Message}");
};

broker.OnNotificationSucceeded += (notification) =>
{
	Console.WriteLine("Notification Sent!");
};

// Start the broker
broker.Start();

// Queue notifications
foreach (var regId in MY_REGISTRATION_IDS)
{
	var notification = new HuaweiNotification();
	notification.Message.token = new string[] { regId };
	notification.Message.data = JObject.Parse("{ \"somekey\" : \"Hello from PushSharp!\" }");

	broker.QueueNotification(notification);
}

// Stop the broker when done
broker.Stop();
```

#### Huawei Configuration Notes

- **Client Secret**: From Huawei AppGallery Connect → Project Settings → App Information
- **Project ID**: For V2 API (recommended)
- **Application ID**: For V1 API (legacy support)

## Additional Platform Support

For Windows Push Notification Services (WNS), Amazon Device Messaging (ADM), and Blackberry push notifications, please refer to the [original PushSharp documentation](https://github.com/Redth/PushSharp).

## Event Handling

All service brokers provide two primary events:

- **OnNotificationSucceeded**: Fired when a notification is successfully sent
- **OnNotificationFailed**: Fired when a notification fails to send

Always implement these handlers to monitor your notification delivery status and handle errors appropriately.

## Best Practices

1. **Reuse Broker Instances**: Create broker instances once and reuse them for multiple notifications
2. **Batch Notifications**: Queue multiple notifications before calling `Stop()`
3. **Handle Expired Tokens**: Listen for `DeviceSubscriptionExpiredException` and remove invalid tokens from your database
4. **Respect Rate Limits**: Handle `RetryAfterException` and implement exponential backoff
5. **Environment Separation**: Use separate configurations for development and production environments
6. **Error Logging**: Implement comprehensive error logging in event handlers

## Troubleshooting

### APNS Issues
- Verify your P8 certificate is valid and not expired
- Ensure the Key ID and Team ID are correct
- Check that the bundle ID matches your app
- Use the correct environment (sandbox vs production)

### Firebase Issues
- Verify the service account JSON is valid
- Ensure the project ID is correct
- Check that device registration tokens are valid and not expired

### Huawei Issues
- Verify client secret and app credentials
- Ensure tokens are valid
- Check that the app is properly registered in AppGallery Connect

## Contributing

Contributions are welcome! Please feel free to submit pull requests or open issues.

## Credits

This is a fork and modernization of the original [PushSharp by Redth](https://github.com/Redth/PushSharp), updated and maintained by [DKorablin](https://github.com/DKorablin).

## Links

- [Original PushSharp Documentation](https://github.com/Redth/PushSharp)
- [Apple Push Notification Service Documentation](https://developer.apple.com/documentation/usernotifications)
- [Firebase Cloud Messaging Documentation](https://firebase.google.com/docs/cloud-messaging)
- [Huawei Push Kit Documentation](https://developer.huawei.com/consumer/en/doc/development/HMSCore-Guides/service-introduction-0000001050040060)