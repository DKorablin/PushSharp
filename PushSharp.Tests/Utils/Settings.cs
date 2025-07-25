﻿using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.IO;
using AlphaOmega.PushSharp.Google;
using AlphaOmega.PushSharp.Apple;

namespace AlphaOmega.PushSharp.Tests.Utils
{
	public class Settings
	{
#if MANUAL_BUILD
		public const String AUTOBUILD_DISABLED = null;
#else
		public const String AUTOBUILD_DISABLED = "Real tests disabled on CI/CD";
#endif

		public static Settings Instance => GetSettingsFilePath<Settings>("settings.json", "TEST_CONFIG_JSON");

		public static FirebaseSettings Firebase => GetSettingsFilePath<FirebaseSettings>("Firebase.ServiceAccount.json", "TEST_FIREBASE_JSON");

		private static T GetSettingsFilePath<T>(String fileName, String environmentKey)
		{
			var envData = Environment.GetEnvironmentVariable(environmentKey);

			if(!String.IsNullOrEmpty(envData))
				return JsonConvert.DeserializeObject<T>(envData);

			var baseDir = AppDomain.CurrentDomain.BaseDirectory;

			var settingsFile = Path.Combine(baseDir, fileName);

			if(!File.Exists(settingsFile))
				settingsFile = Path.Combine(baseDir, @"..\", fileName);
			if(!File.Exists(settingsFile))
				settingsFile = Path.Combine(baseDir, @"..\..\", fileName);
			if(!File.Exists(settingsFile))
				settingsFile = Path.Combine(baseDir, @"..\..\..\", fileName);

			return File.Exists(settingsFile)
				? JsonConvert.DeserializeObject<T>(File.ReadAllText(settingsFile))
				: throw new FileNotFoundException($"You must provide a {fileName} file to run these tests. See the {fileName}.sample file for more information.");
		}

		public Settings()
		{
		}

		[JsonProperty("apns_p8cert_file")]
		public String ApnsCertificateFile { get; set; }

		[JsonProperty("apns_p8cert_keyId")]
		public String ApnsCertificateKeyId { get; set; } = nameof(ApnsSettings.KeyId);

		[JsonProperty("apns_teamId")]
		public String ApnsTeamId { get; set; } = nameof(ApnsSettings.TeamId);

		[JsonProperty("apns_bundleId")]
		public String ApnsBundleId { get; set; } = nameof(ApnsSettings.AppBundleId);

		[JsonProperty("apns_device_tokens")]
		public List<String> ApnsDeviceTokens { get; set; } = new List<String>();

		[JsonProperty("gcm_auth_token")]
		public String GcmAuthToken { get; set; }

		[JsonProperty("gcm_sender_id")]
		public String GcmSenderId { get; set; }

		[JsonProperty("gcm_registration_ids")]
		public List<String> GcmRegistrationIds { get; set; }

		[JsonProperty("adm_client_id")]
		public String AdmClientId { get; set; }

		[JsonProperty("adm_client_secret")]
		public String AdmClientSecret { get; set; }

		[JsonProperty("adm_registration_ids")]
		public List<String> AdmRegistrationIds { get; set; }

		[JsonProperty("wns_package_name")]
		public String WnsPackageName { get; set; }

		[JsonProperty("wns_package_sid")]
		public String WnsPackageSid { get; set; }

		[JsonProperty("wns_client_secret")]
		public String WnsClientSecret { get; set; }

		[JsonProperty("wns_channel_uris")]
		public List<String> WnsChannelUris { get; set; }

		[JsonProperty("huawei_client_id")]
		public String HuaweiClientId { get; set; }

		[JsonProperty("huawei_client_secret")]
		public String HuaweiClientSecret { get; set; }

		[JsonProperty("huawei_project_id")]
		public String HuaweiProjectId { get; set; }

		[JsonProperty("huawei_app_id")]
		public String HuaweiApplicationId { get; set; }

		[JsonProperty("huawei_registration_ids")]
		public List<String> HuaweiRegistrationIds { get; set; }
	}
}