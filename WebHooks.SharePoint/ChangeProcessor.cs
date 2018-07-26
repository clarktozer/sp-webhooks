using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Linq;
using WebHooks.Data;
using WebHooks.Data.Models;

namespace WebHooks.SharePoint
{
    public interface IChangeProcesser
    {
        void Process();
    }

    public class ChangeProcessor : IChangeProcesser
    {
        private readonly ILogger<ChangeProcessor> _logger;
        private readonly IQueueContext _queueContext;
        private readonly ITokenProcessor _tokenProcessor;
        private readonly IListProcessor _listProcessor;

        public ChangeProcessor(
            ILogger<ChangeProcessor> logger, 
            IQueueContext queueContext, 
            ITokenProcessor tokenProcessor,
            IListProcessor listProcessor)
        {
            _logger = logger;
            _queueContext = queueContext;
            _tokenProcessor = tokenProcessor;
            _listProcessor = listProcessor;
        }

        public void Process()
        {
            _queueContext.OnPop<Notification>((notification) =>
            {
                ProcessNotificationAsync(notification);
            });
        }

        private async void ProcessNotificationAsync(Notification notification)
        {
            try
            {
                _logger.LogInformation($"Processing notification: {JsonConvert.SerializeObject(notification)}");
                _tokenProcessor.Process(notification);
                var changeSet = await _listProcessor.GetListChanges(notification, _tokenProcessor.LastChangeToken);
                await _tokenProcessor.SaveTokens(changeSet.Changes.Last().ChangeToken, notification);
                _listProcessor.UpdateExpiry(notification, changeSet.List);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
            }
        }
    }
}
