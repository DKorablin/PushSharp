using System.Collections.Generic;
using PushSharp.Amazon;
using Xunit;

namespace PushSharp.Tests
{
    [Collection(Settings.DISABLED)]
    public class AdmRealTests
    {
        [Fact(Skip = Settings.DISABLED)]
        public void ADM_Send_Single()
        {
            var succeeded = 0;
            var failed = 0;
            var attempted = 0;

            var config = new AdmConfiguration(Settings.Instance.AdmClientId, Settings.Instance.AdmClientSecret);
            var broker = new AdmServiceBroker(config);
            broker.OnNotificationFailed += (notification, exception) => failed++;
            broker.OnNotificationSucceeded += (notification) => succeeded++;
            broker.Start();

            foreach(var regId in Settings.Instance.AdmRegistrationIds)
            {
                attempted++;
                broker.QueueNotification(new AdmNotification
                {
                    RegistrationId = regId,
                    Data = new Dictionary<string, string> {
                        { "somekey", "somevalue" }
                    }
                });
            }

            broker.Stop();

            Assert.Equal(attempted, succeeded);
            Assert.Equal(0, failed);
        }
    }
}