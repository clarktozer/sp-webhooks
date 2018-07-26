using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.SharePoint.Client;
using Newtonsoft.Json;
using OfficeDevPnP.Core;
using WebHooks.Data;
using WebHooks.Data.Models;

namespace WebHooks.SharePoint
{
    public interface IChangeHandler
    {
        void Process();
    }

    public class ChangeHandler : IChangeHandler
    {
        private readonly ILogger<ChangeHandler> _logger;
        private readonly IConfiguration _configuration;
        private ChangeTokenContext _dbContext;
        private AuthenticationManager _authenticationManager;
        private readonly IQueueContext _queueContext;
        private string _accessToken;

        public ChangeHandler(ILogger<ChangeHandler> logger, IConfiguration configuration, IQueueContext queueContext, ChangeTokenContext dbContext)
        {
            _logger = logger;
            _configuration = configuration;
            _queueContext = queueContext;
            _dbContext = dbContext;
            _authenticationManager = new AuthenticationManager();
        }

        public void Process()
        {
            _queueContext.OnPop<Notification>((notification) =>
            {
                var tenant = _configuration["SharePointAppSettings:Tenant"];
                var clientId = _configuration["SharePointAppSettings:ClientId"];
                var clientSecret = _configuration["SharePointAppSettings:ClientSecret"];
                var url = $"https://{tenant}{notification.SiteUrl}";

                var context = _authenticationManager.GetAppOnlyAuthenticatedContext(url, clientId, clientSecret);
                ProcessNotificationAsync(context, notification);
            });
        }

        private async void ProcessNotificationAsync(ClientContext context, Notification notification)
        {
            _logger.LogInformation($"Processing notification: {JsonConvert.SerializeObject(notification)}");
            context.ExecutingWebRequest += AssignAccessToken;
            var webGuid = new Guid(notification.WebId);
            var web = context.Site.OpenWebById(webGuid);
            context.Load(web);
            context.Load(context.Web);
            var listGuid = new Guid(notification.Resource);
            var changeList = web.Lists.GetById(listGuid);
            context.Load(changeList, t => t.Title);
            await context.ExecuteQueryAsync();

            _logger.LogInformation($"Processing notification: {changeList.Title}");
        }

        private void AssignAccessToken(object sender, WebRequestEventArgs e)
        {
            _accessToken = e.WebRequestExecutor.RequestHeaders.Get("Authorization").Replace("Bearer ", "");
        }
    }
}
