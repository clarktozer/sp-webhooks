using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using WebHooks.Data;
using WebHooks.SharePoint;

namespace WebHooks.Job
{
    public class Program
    {
        private static IConfiguration _configuration;
        private static IServiceProvider _serviceProvider;

        public static void Main(string[] args)
        {
            AddConfiguration();
            AddServiceCollection();
            var changeProcessor = _serviceProvider.GetService<IChangeProcesser>();
            changeProcessor.Process();
            RunAndBlock();
        }

        private static void AddConfiguration()
        {
            var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env}.json", optional: true)
                .AddEnvironmentVariables();

            _configuration = builder.Build();
        }

        private static void AddServiceCollection()
        {
            var changeTokensConnection = _configuration.GetConnectionString($"ChangeTokens");

            var serviceProvider = new ServiceCollection()
                .AddLogging()
                .AddDbContext<ChangeTokenContext>(options => options.UseSqlServer(changeTokensConnection))
                .AddSingleton(_configuration)
                .AddTransient<IQueueContext, QueueContext>()
                .AddTransient<IChangeProcesser, ChangeProcessor>()
                .AddTransient<ITokenProcessor, TokenProcessor>()
                .AddTransient<IListProcessor, ListProcessor>()
                .BuildServiceProvider();

            serviceProvider
                .GetService<ILoggerFactory>()
                .AddConsole(_configuration.GetSection("Logging"));

            _serviceProvider = serviceProvider;
        }

        private static void RunAndBlock()
        {
            while (true) { }
        }
    }
}
