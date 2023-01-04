using System;
using System.Threading.Tasks;
using Amazon;
using Amazon.Runtime;
using Amazon.SimpleNotificationService;
using CloudNotes.Models;
using CloudNotes.Models.HttpEndpoint;
using CloudNotes.Models.Settings;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace CloudNotes.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SNSNotificationController : ControllerBase
    {
        private readonly ILogger<SNSNotificationController> _logger;
        private readonly SNSSettings _snsSettings;
        private readonly IAmazonSimpleNotificationService _snsClient;

        public SNSNotificationController(
            IOptions<SNSSettings> snsSettings,
            ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<SNSNotificationController>();
            _snsSettings = snsSettings.Value;

            var region = RegionEndpoint.GetBySystemName(_snsSettings.AWSRegion);

            _snsClient = new AmazonSimpleNotificationServiceClient(
                new BasicAWSCredentials(
                    _snsSettings.AWSAccessKey,
                    _snsSettings.AWSSecretKey),
                region
            );
        }

        [HttpPost]
        public async Task<ActionResult> HandleSNSMessage([FromBody] SNSEndpointModel model)
        {
            try
            {
                // check message type
                switch (model.Type)
                {
                    case "SubscriptionConfirmation":
                        await _snsClient.ConfirmSubscriptionAsync(
                            model.TopicArn,
                            model.Token
                            );
                        break;
                    case "Notification":
                        
                        _logger.LogInformation($"New notification received");
                        _logger.LogInformation(model.Message);
                        Event message = JsonConvert.DeserializeObject<Event>(model.Message);
                        _logger.LogInformation($"New notification received for note id: '{message.NoteId}'");

                        // TODO: Do interesting work based on the new message

                        // throw new Exception("Fake exception to trigger redrive policy");
                        break;
                    case "UnsubscribeConfirmation":
                        // Handle unsubscription if needed
                        break;
                    default:
                        _logger.LogError($"Unexpected message type for message: '{JsonConvert.SerializeObject(model)}'");
                        throw new ArgumentOutOfRangeException($"Unexpected message type '{model.Type}'");
                }
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, "Couldn't process notification");
                throw;
            }

            return Ok();
        }
    }
}
