using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Threading.Tasks;
using Messages;
using NServiceBus;
using NServiceBus.Logging;

namespace Facility
{
    class Program
    {
        static void Main(string[] args)
        {
            ServicePointManager.DefaultConnectionLimit = 5000; // default settings only allows 2 concurrent requests per process to the same host
            ServicePointManager.UseNagleAlgorithm = false; // optimize for small requests
            ServicePointManager.Expect100Continue = false; // reduces number of http calls
            ServicePointManager.CheckCertificateRevocationList = false; // optional, only disable if all dependencies are trusted 

            var stopWatch = new Stopwatch();
            stopWatch.Start();
            DefaultFactory defaultFactory = LogManager.Use<DefaultFactory>();
            defaultFactory.Level(LogLevel.Error);

            var configuration = new EndpointConfiguration("Chocolate.Facility");

            configuration.UseTransport<AzureStorageQueueTransport>().ConnectionString("DefaultEndpointsProtocol=http;AccountName=nsbperfv6;AccountKey=o7kCW0G7TmNNN7aA23PUj06BCGb9qvCQ02YfeB64QqwJpVFhxbGiYq31+/piTSa7swhQpTlKioNnHplKrbn7oA==;");
            configuration.UsePersistence<InMemoryPersistence>();
            configuration.UseSerialization<JsonSerializer>();
            configuration.EnableInstallers();
            configuration.SendOnly();

            var bus = Endpoint.Start(configuration).GetAwaiter().GetResult();
            stopWatch.Stop();

            Console.WriteLine($"Initalizing the bus took { stopWatch.Elapsed.ToString("G")}");
            stopWatch.Reset();

            var destination = "Chocolate.Facility.Producer";
            stopWatch.Start();

            var tasks = new List<Task>();
            for (int i = 0; i < Constants.NumberOfMessages; i++)
            {
                tasks.Add(bus.Send(destination, new ProduceChocolateBar { LotNumber = i, MaxLotNumber = Constants.NumberOfMessages }));
            }

            Task.WhenAll(tasks).GetAwaiter().GetResult();

            stopWatch.Stop();
            Console.WriteLine($"Sending #{ Constants.NumberOfMessages } of msgs over the bus took { stopWatch.Elapsed.ToString("G")}");

            Console.ReadLine();
        }
    }
}
