using Amazon;
using Amazon.Runtime;
using Amazon.SimpleNotificationService;
using Amazon.SimpleNotificationService.Model;
using CloudNotes.Models;
using CloudNotes.Models.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CloudNotes.Services
{
    public class SNSEventPublisher : IEventPublisher
    {
        private readonly ILogger _logger;
        private readonly SNSSettings _snsSettings;
        private readonly IAmazonSimpleNotificationService _snsClient;

        public SNSEventPublisher(
            IOptions<SNSSettings> snsSettings,
            ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<SNSEventPublisher>();
            _snsSettings = snsSettings.Value;

            var region = RegionEndpoint.GetBySystemName(_snsSettings.AWSRegion);

            _snsClient = new AmazonSimpleNotificationServiceClient(
                new BasicAWSCredentials(
                    _snsSettings.AWSAccessKey,
                    _snsSettings.AWSSecretKey),
                region
            );
        }

        public async Task PublishEvent(Event eventData)
        {
            try
            {
                PublishRequest publishRequest = new PublishRequest();

                SNSMessage message = new SNSMessage(JsonConvert.SerializeObject(eventData));
                message.EmailJsonContent = $"{eventData.EventType.ToString()} - Note {eventData.NoteId}";               

                publishRequest.TopicArn = _snsSettings.TopicArn;
                publishRequest.MessageStructure = "json";
                publishRequest.Message = message.ToString();
                publishRequest.Subject = $"CloudNotes - {eventData.EventType.ToString()} - Note {eventData.NoteId}";

                SNSMessageAttributeCollection messageAttributes = new SNSMessageAttributeCollection();
                messageAttributes.Add("Publisher", "CloudNotes Web");
                messageAttributes.Add("AWS.SNS.SMS.SenderID", "Cloudnotes");
                messageAttributes.Add("AWS.SNS.SMS.SMSType", "Transactional");
                messageAttributes.Add("AWS.SNS.SMS.MaxPrice", 0.05);
                messageAttributes.Add("AWS.SNS.MOBILE.FCM.TTL", "86400");
                publishRequest.MessageAttributes = messageAttributes;

                PublishResponse publishResponse = await _snsClient.PublishAsync(publishRequest);
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, "Couldn't publish an event to SNS");
                throw;
            }
        }
    }
}
