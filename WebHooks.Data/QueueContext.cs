using Microsoft.Azure.ServiceBus;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace WebHooks.Data
{
    public interface IQueueContext
    {
        Task Push<T>(T item);
        void OnPop(MessageHandler handler);
    }

    public delegate void MessageHandler(Message msg, CancellationToken token);

    public class QueueContext : IQueueContext
    {
        private string ConnectionString { get; }
        private string QueueName { get; }
        private readonly ILogger<QueueContext> _logger;

        public QueueContext(ILogger<QueueContext> logger, IConfiguration configuration)
        {
            _logger = logger;
            ConnectionString = configuration.GetConnectionString($"Queue");
            QueueName = configuration["Queue:Name"];
        }

        public void OnPop(MessageHandler handler)
        {
            QueueClient client = new QueueClient(ConnectionString, QueueName);
            client.RegisterMessageHandler(async (msg, token) =>
            {
                handler(msg, token);
                await client.CompleteAsync(msg.SystemProperties.LockToken);
            }, new MessageHandlerOptions(ExceptionReceivedHandler)
            {
                MaxConcurrentCalls = 1,
                AutoComplete = false
            });
        }

        public async Task Push<T>(T item)
        {
            _logger.LogDebug("Push notification to queue {0}", QueueName);
            QueueClient client = new QueueClient(ConnectionString, QueueName);
            string serialized = JsonConvert.SerializeObject(item);
            Message message = new Message(Encoding.UTF8.GetBytes(serialized))
            {
                ContentType = "application/json"
            };
            await client.SendAsync(message);
        }

        private Task ExceptionReceivedHandler(ExceptionReceivedEventArgs exceptionReceivedEventArgs)
        {
            Console.WriteLine($"Message handler exception: {exceptionReceivedEventArgs.Exception}.");
            var context = exceptionReceivedEventArgs.ExceptionReceivedContext;
            Console.WriteLine($"---- Endpoint: {context.Endpoint}");
            Console.WriteLine($"---- Entity Path: {context.EntityPath}");
            Console.WriteLine($"---- Executing Action: {context.Action}");
            Console.WriteLine($"---- Client Id: {context.ClientId}");
            return Task.CompletedTask;
        }
    }
}
