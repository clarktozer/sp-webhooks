using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Text;
using WebHooks.Data;
using WebHooks.Data.Models;
using Microsoft.EntityFrameworkCore;
using WebHooks.SharePoint;

namespace WebHooks.Job
{
    public class Program
    {
        private static IConfiguration _configuration;
        private static IServiceProvider _serviceProvider;

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
                .AddTransient<IChangeHandler, ChangeHandler>()
                .BuildServiceProvider();

            serviceProvider
                .GetService<ILoggerFactory>()
                .AddConsole(_configuration.GetSection("Logging"));

            _serviceProvider = serviceProvider;
        }

        public static void Main(string[] args)
        {
            AddConfiguration();
            AddServiceCollection();
            var changeHandler = _serviceProvider.GetService<IChangeHandler>();
            changeHandler.Process();
            RunAndBlock();
        }

        private static void RunAndBlock()
        {
            while (true) { }
        }
    }
}
