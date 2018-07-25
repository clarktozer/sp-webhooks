using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WebHooks.Data.Models;

namespace WebHooks.Web.Controllers
{
    [Route("api/Webhook")]
    public class WebhookController : Controller
    {
        public WebhookController()
        {

        }

        [HttpPost("consume")]
        public async Task<IActionResult> Consume([FromQuery] string validationToken = null, Response<Notification> response = null)
        {
            if (validationToken != null)
            {
                Trace.TraceInformation("Subscription added with validationToken {0}", validationToken);
                return Content(validationToken);
            }

            try
            {
                if (response != null && response.Value.Count > 0)
                {

                }
            }
            catch (Exception ex)
            {
                Trace.TraceError(ex.Message);
            }

            return Ok();
        }
    }
}