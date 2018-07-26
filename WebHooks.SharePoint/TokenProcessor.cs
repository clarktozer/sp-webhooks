using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using WebHooks.Data;
using WebHooks.Data.Models;

namespace WebHooks.SharePoint
{
    public interface ITokenProcessor
    {
        ChangeToken CurrentToken { get; }
        Microsoft.SharePoint.Client.ChangeToken LastChangeToken { get; }
        void Process(Notification notification);
        Task SaveTokens(Microsoft.SharePoint.Client.ChangeToken LastChangeToken, Notification notification);
    }

    public class TokenProcessor : ITokenProcessor
    {
        public ChangeToken CurrentToken { get; private set; }
        private ChangeTokenContext _dbContext;
        private readonly ILogger<TokenProcessor> _logger;
        public Microsoft.SharePoint.Client.ChangeToken LastChangeToken { get; private set; }

        public TokenProcessor(ILogger<TokenProcessor> logger, ChangeTokenContext dbContext)
        {
            _logger = logger;
            _dbContext = dbContext;
        }

        public void Process(Notification notification)
        {
            CurrentToken = _dbContext.GetLatestBySubscriptionId(notification.SubscriptionId);

            if (CurrentToken != null)
            {
                _logger.LogInformation($"Get previous change token: {CurrentToken.LastChangeToken}");
                LastChangeToken = new Microsoft.SharePoint.Client.ChangeToken
                {
                    StringValue = CurrentToken.LastChangeToken
                };
            }
        }

        public async Task SaveTokens(Microsoft.SharePoint.Client.ChangeToken lastChangeToken, Notification notification)
        {
            LastChangeToken = lastChangeToken;

            if (CurrentToken != null)
            {
                if (!CurrentToken.LastChangeToken.Equals(LastChangeToken.StringValue, StringComparison.InvariantCultureIgnoreCase))
                {
                    _logger.LogInformation($"Save latest change token to DB: {LastChangeToken.StringValue}");
                    CurrentToken.LastChangeToken = LastChangeToken.StringValue;
                }
            }
            else
            {
                _logger.LogInformation($"Save new change token to DB: {LastChangeToken.StringValue} for subscription {notification.SubscriptionId}");
                _dbContext.ChangeTokens.Add(new ChangeToken
                {
                    Id = Guid.Parse(notification.SubscriptionId),
                    ListId = notification.Resource,
                    WebId = notification.WebId,
                    LastChangeToken = LastChangeToken.StringValue
                });

            }

            await _dbContext.SaveChangesAsync();
        }
    }

}
