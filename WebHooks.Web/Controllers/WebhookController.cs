using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using WebHooks.Data.Models;

namespace WebHooks.Web.Controllers
{
    [Route("api/Webhook")]
    public class WebhookController : Controller
    {
        private readonly ILogger _logger;

        public WebhookController(ILogger<WebhookController> logger)
        {
            _logger = logger;
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
                    var subIds = response.Value.Select(notification => notification.SubscriptionId);
                    _logger.LogDebug("Notifications received for ids: {0}", string.Join(", ", subIds));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
            }

            return Ok();
        }
    }
}