using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.SharePoint.Client;
using OfficeDevPnP.Core;
using OfficeDevPnP.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebHooks.Data.Models;
using WebHooks.SharePoint.Models;

namespace WebHooks.SharePoint
{
    public interface IListProcessor
    {
        Task<ChangeSet> GetListChanges(Notification notification, Microsoft.SharePoint.Client.ChangeToken lastChangeToken);
        void UpdateExpiry(Notification notification, List changesList);
    }

    public class ListProcessor : IListProcessor
    {
        private readonly ILogger<ListProcessor> _logger;
        private readonly IConfiguration _configuration;
        private AuthenticationManager _authenticationManager;
        private string _accessToken;

        public ListProcessor(ILogger<ListProcessor> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
            _authenticationManager = new AuthenticationManager();
        }

        public void UpdateExpiry(Notification notification, List changesList)
        {
            if (notification.ExpirationDateTime.AddDays(-5) >= DateTime.Now) return;
            var subscription = new WebhookSubscription
            {
                Id = notification.SubscriptionId,
                ClientState = notification.ClientState,
                ExpirationDateTime = notification.ExpirationDateTime.AddDays(90),
                NotificationUrl = _configuration.GetConnectionString("WebhookConsumer"),
                Resource = notification.Resource
            };

            _logger.LogInformation($"Change token has nearly expired for {notification.SubscriptionId}. Updating expiry by 90 days.");
            var updateResult = changesList.UpdateWebhookSubscription(subscription, _accessToken);

            if (!updateResult)
            {
                throw new Exception($"The expiration date of web hook {notification.SubscriptionId} with endpoint {_configuration.GetConnectionString("WebhookConsumer")} could not be updated");
            }
        }

        public async Task<ChangeSet> GetListChanges(Notification notification, Microsoft.SharePoint.Client.ChangeToken lastChangeToken)
        {
            var context = GetContext(notification.SiteUrl);
            var webGuid = new Guid(notification.WebId);
            var web = context.Site.OpenWebById(webGuid);
            var listGuid = new Guid(notification.Resource);
            var changeList = web.Lists.GetById(listGuid);

            context.Load(web);
            context.Load(changeList);
            await context.ExecuteQueryAsync();

            var changeQuery = new ChangeQuery()
            {
                Item = true,
                Update = true,
                Add = true,
                DeleteObject = true
            };

            if (lastChangeToken != null)
            {
                changeQuery.ChangeTokenStart = lastChangeToken;
            }
            else
            {
                changeQuery.ChangeTokenStart = new Microsoft.SharePoint.Client.ChangeToken
                {
                    StringValue = $"1;3;{notification.Resource};{DateTime.Now.AddMinutes(-5).ToUniversalTime().Ticks};-1"
                };
                _logger.LogInformation($"Get new change token: {changeQuery.ChangeTokenStart.StringValue}");
            }

            var changes = changeList.GetChanges(changeQuery);
            context.Load(changes);
            await context.ExecuteQueryAsync();

            var changeSet = new ChangeSet()
            {
                Web = web,
                List = changeList,
                Changes = GetUniqueChanges(changes)
            };

            PrintChanges(changeSet);
            return changeSet;
        }

        private ClientContext GetContext(string siteUrl)
        {
            var tenant = _configuration["SharePointAppSettings:Tenant"];
            var clientId = _configuration["SharePointAppSettings:ClientId"];
            var clientSecret = _configuration["SharePointAppSettings:ClientSecret"];
            var url = $"https://{tenant}{siteUrl}";
            return _authenticationManager.GetAppOnlyAuthenticatedContext(url, clientId, clientSecret);
        }

        private List<Change> GetUniqueChanges(ChangeCollection changes)
        {
            return changes
                .GroupBy(i => ((ChangeItem)i).ItemId,
                    (key, g) =>
                        g.OrderByDescending(e => e.Time).First()).OrderByDescending(c => c.Time)
                .ToList();
        }

        private void PrintChanges(ChangeSet changeSet)
        {
            if (changeSet.Changes.Any())
            {
                foreach (var change in changeSet.Changes)
                {
                    var changeItem = (ChangeItem)change;
                    _logger.LogInformation($"A Change of type {change.ChangeType.ToString()} occurred on item {changeItem.ItemId} at {change.Time.ToString("dd/MM/yyyy H:mm:ss")}");
                }
            }
        }

        private void AssignAccessToken(object sender, WebRequestEventArgs e)
        {
            _accessToken = e.WebRequestExecutor.RequestHeaders.Get("Authorization").Replace("Bearer ", "");
        }
    }
}
