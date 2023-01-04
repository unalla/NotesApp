using Newtonsoft.Json;

namespace CloudNotes.Models
{
    public class SNSMessage
    {
        [JsonProperty("default")]
        public string DefaultContent { get; }
        
        [JsonProperty("email")]
        public string EmailContent { get; set; }

        [JsonProperty("email-json")]
        public string EmailJsonContent { get; set; }

        [JsonProperty("sqs")]
        public string SQSContent { get; set; }
        
        [JsonProperty("lambda")]
        public string LambdaContent { get; set; }
        
        [JsonProperty("http")]
        public string HttpContent { get; set; }
        
        [JsonProperty("https")]
        public string HttpsContent { get; set; }
        
        [JsonProperty("sms")]
        public string SMSContent { get; set; }
        
        [JsonProperty("APNS")]
        public string APNSContent { get; set; }
        
        [JsonProperty("APNS_SANDBOX")]
        public string APNSSandboxContent { get; set; }
        
        [JsonProperty("MACOS")]
        public string MacOsContent { get; set; }
        
        [JsonProperty("MACOS_SANDBOX")]
        public string MacOsSandboxContent { get; set; }
        
        [JsonProperty("GCM")]
        public string FCMContent { get; set; }
        
        [JsonProperty("ADM")]
        public string ADMContent { get; set; }
        
        [JsonProperty("BAIDU")]
        public string BaiduContent { get; set; }
        
        [JsonProperty("MPNS")]
        public string MPNSContent { get; set; }
        
        [JsonProperty("WNS")]
        public string WNSContent { get; set; }

        public SNSMessage(string defaultContent)
        {
            DefaultContent = defaultContent;
        }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}
