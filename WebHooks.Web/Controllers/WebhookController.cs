using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using WebHooks.Data;
using WebHooks.Data.Models;

namespace WebHooks.Web.Controllers
{
    [Route("api/Webhook")]
    public class WebhookController : Controller
    {
        private readonly ILogger _logger;
        private readonly IQueueContext _queue;

        public WebhookController(ILogger<WebhookController> logger, IQueueContext queue)
        {
            _logger = logger;
            _queue = queue;
        }

        [HttpPost("consume")]
        public async Task<IActionResult> Consume([FromQuery] string validationToken = null, Response<Notification> response = null)
        {
            if (validationToken != null)
            {
                _logger.LogDebug("Subscription added with validationToken {0}", validationToken);
                return Content(validationToken);
            }

            try
            {
                if (response != null && response.Value.Count > 0)
                {
                    _logger.LogDebug("Notifications received for ids: {0}", string.Join(", ", response.Value.Select(notification => notification.SubscriptionId)));
                    foreach (var notification in response.Value)
                    {
                        await _queue.Push(notification);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return BadRequest(ex);
            }

            return Ok();
        }
    }
}