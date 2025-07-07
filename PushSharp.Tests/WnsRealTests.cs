using System.Xml.Linq;
using AlphaOmega.PushSharp.Windows;
using Xunit;

namespace AlphaOmega.PushSharp.Tests
{
    [Collection ("Disabled")]
    public class WnsRealTests
    {
        [Fact]
        public void WNS_Send_Single ()
        {
            var succeeded = 0;
            var failed = 0;
            var attempted = 0;

            var config = new WnsConfiguration (Settings.Instance.WnsPackageName,
                             Settings.Instance.WnsPackageSid,
                             Settings.Instance.WnsClientSecret);

            var broker = new WnsServiceBroker (config);
            broker.OnNotificationFailed += (notification, exception) => {
                failed++;
            };
            broker.OnNotificationSucceeded += (notification) => {
                succeeded ++;
            };

            broker.Start ();

            foreach (var uri in Settings.Instance.WnsChannelUris) {
                attempted++;
                broker.QueueNotification (new WnsToastNotification {
                    ChannelUri = uri,
                    Payload = XElement.Parse (@"
                        <toast>
                            <visual>
                                <binding template=""ToastText01"">
                                    <text id=""1"">WNS_Send_Single</text>
                                </binding>  
                            </visual>
                        </toast>
                    ")
                });
            }

            broker.Stop ();

            Assert.Equal (attempted, succeeded);
            Assert.Equal (0, failed);
        }

        [Fact]
        public void WNS_Send_Mutiple ()
        {
            var succeeded = 0;
            var failed = 0;
            var attempted = 0;

            var config = new WnsConfiguration (Settings.Instance.WnsPackageName,
                Settings.Instance.WnsPackageSid,
                Settings.Instance.WnsClientSecret);

            var broker = new WnsServiceBroker (config);
            broker.OnNotificationFailed += (notification, exception) => {
                failed++;
            };
            broker.OnNotificationSucceeded += (notification) => {
                succeeded++;
            };

            broker.Start ();

            foreach (var uri in Settings.Instance.WnsChannelUris) {
                for (var i = 1; i <= 3; i++) {
                    attempted++;
                    broker.QueueNotification (new WnsToastNotification {
                            ChannelUri = uri,
                            Payload = XElement.Parse(@"
                                <toast>
                                    <visual>
                                        <binding template=""ToastText01"">
                                            <text id=""1"">WNS_Send_Multiple " + i.ToString() + @"</text>
                                        </binding>  
                                    </visual>
                                </toast>
                            ")
                        });
                }
            }

            broker.Stop ();

            Assert.Equal (attempted, succeeded);
            Assert.Equal (0, failed);
        }
    }
}