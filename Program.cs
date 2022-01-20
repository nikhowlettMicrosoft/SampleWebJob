using Microsoft.Azure.WebJobs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration.Memory;
using Azure.Messaging.ServiceBus;
using Azure.Identity;

namespace WebJobTest
{
    // To learn more about Microsoft Azure WebJobs SDK, please see https://go.microsoft.com/fwlink/?LinkID=320976
    class Program
    {
        // Please set the following connection strings in app.config for this WebJob to run:
        // AzureWebJobsDashboard and AzureWebJobsStorage
        static void Main()
        {
            var builder = new HostBuilder();
            var settingsAsCollection = ConfigurationManager.AppSettings;
            var settings = settingsAsCollection.AllKeys.ToDictionary(k => k, k => settingsAsCollection[k]);
            settings["ServiceBus:fullyQualifiedNamespace"] = settings["Microsoft.ServiceBus.ConnectionString"];


            var storageConnection = ConfigurationManager.ConnectionStrings["AzureWebJobsDashboard"].ConnectionString;
            settings["AzureWebJobsDashboard"] = storageConnection;
            settings["AzureWebJobsStorage"] = storageConnection;

            builder.ConfigureAppConfiguration(x =>
            x.Add(new MemoryConfigurationSource()
            {
                InitialData = new Dictionary<string, string>
                {
                    {$"ServiceBus:fullyQualifiedNamespace", settings["Microsoft.ServiceBus.ConnectionString"]}
                }
            }));
            builder.ConfigureAppConfiguration((context, config) =>
            {
                config.AddInMemoryCollection(settings);
                var apples = config.Build();
            });
            builder.ConfigureWebJobs(b =>
            {
                b.AddAzureStorageCoreServices();
                b.AddServiceBus();
                b.AddAzureStorageBlobs();
                b.AddAzureStorageQueues();
                b.AddFiles();
                b.AddTimers();
            })
            .ConfigureServices((hostContext, services) =>
            {
                services.AddHttpClient();
                services.AddSingleton<Receiver>();
            })
            .UseConsoleLifetime();

            System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls12;
            var host = builder.Build();

            using (host)
            {
                host.Run();
            }
        }
    }
}
