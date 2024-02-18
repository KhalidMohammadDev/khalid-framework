using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Khalid.Core.Framework
{
    public class MobileNotificationService
    {
        private readonly MobileConfiguration configuration;
        public MobileNotificationService(MobileConfiguration mobileConfiguration)
        {
            configuration = mobileConfiguration;
        }
        public async Task PushNotification(MobileNotification message, string proxyAddress = null, bool throWhenFaild = false, object extraData = null, int type = 0)
        {

            //new Thread(() =>
            //{
            try
            {
                string applicationID = message.ApplicationId;
                string senderId = message.SenderId;
                //string deviceId = "fEAr-6fhyWw:APA91bHZiO_e6el0pEqxjWFHNGUPtUC2hrFmvzZl9UcfEpsIxfy90dePpXr8dT7A0S6fEGJvG2YYzxEvLqgKysbHnz0ld25XG6ac8TKi0tM9O6R0u7Qr2VDNANm4-_PA2HaTwbqGaxjO";
                WebRequest tRequest = WebRequest.Create("https://fcm.googleapis.com/fcm/send");
                tRequest.Method = "post";
                tRequest.ContentType = "application/json";
                //tRequest.Timeout = 60000;

                var data = new
                {
                    registration_ids = message.Tokens.Distinct().ToList(),
                    notification = new
                    {
                        body = message.MessageBody,
                        title = message.MessageTitle,
                        sound = "Enabled",
                        notification_type = type,
                        extra_data = JsonConvert.SerializeObject(extraData)
                    },
                    data = new
                    {
                        data_title = message.MessageTitle,
                        data_body = message.MessageBody,
                        notification_type = type,
                        click_action = "FLUTTER_NOTIFICATION_CLICK",
                        extra_data = JsonConvert.SerializeObject(extraData)
                    }

                };

                //var serializer = new JavaScriptSerializer();
                var json = JsonConvert.SerializeObject(data);
                Byte[] byteArray = Encoding.UTF8.GetBytes(json);
                tRequest.Headers.Add(string.Format("Authorization: key={0}", applicationID));
                tRequest.Headers.Add(string.Format("Sender: id={0}", senderId));
                tRequest.ContentLength = byteArray.Length;
                if (!String.IsNullOrWhiteSpace(proxyAddress))
                {
                    tRequest.Proxy = new WebProxy(proxyAddress, false);
                }


                using (Stream dataStream = await tRequest.GetRequestStreamAsync())
                {
                    await dataStream.WriteAsync(byteArray, 0, byteArray.Length);
                    using (WebResponse tResponse = await tRequest.GetResponseAsync())
                    {
                        using (Stream dataStreamResponse = tResponse.GetResponseStream())
                        {
                            using (StreamReader tReader = new StreamReader(dataStreamResponse))
                            {
                                String sResponseFromServer = await tReader.ReadToEndAsync();
                                string str = sResponseFromServer;
                            }
                        }
                    }
                }
           
            }

           
            catch (Exception ex)
            {

                if (throWhenFaild)
                    throw;
#if DEBUG
                throw;
#endif
                //string str = ex.Message;
            }
        }

        public async Task PushNotificationAsync(List<string> tokens, string title, string body, string proxyAddress = null, bool throWhenFaild = false, object extraData = null, int type = 0)
        {
            if (string.IsNullOrWhiteSpace(configuration.ApplicationId))
            {
                throw new Exception("You must povide ApplicationId in app settings to push notification");
            }

            if (string.IsNullOrWhiteSpace(configuration.SenderId))
            {
                throw new Exception("You must povide SenderId in app settings to push notification");
            }
            proxyAddress = configuration.ProxyServer;
#if DEBUG
            proxyAddress = null;
#endif

            await PushNotification(new MobileNotification
            {
                ApplicationId = configuration.ApplicationId,
                SenderId = configuration.SenderId,
                MessageBody = body,
                MessageTitle = title,
                Tokens = tokens
            }, proxyAddress, throWhenFaild, extraData, type);
        }
    }

    public class MobileNotification
    {
        public List<string> Tokens { get; set; }

        public string MessageTitle { get; set; }

        public string MessageBody { get; set; }

        public string SenderId { get; set; }

        public string ApplicationId { get; set; }

    }


    public class MobileConfiguration
    {
        public string ApplicationId { get; set; }

        public string SenderId { get; set; }

        public string ProxyServer { get; set; }

    }

}
