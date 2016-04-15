using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NServiceBus;
using NServiceBus.Logging;

namespace Facility.Producer
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
            defaultFactory.Level(LogLevel.Fatal);

            var configuration = new EndpointConfiguration("Chocolate.Facility.Producer");

            configuration.UseTransport<AzureStorageQueueTransport>()
                .ConnectionString("DefaultEndpointsProtocol=http;AccountName=nsbperfv6;AccountKey=o7kCW0G7TmNNN7aA23PUj06BCGb9qvCQ02YfeB64QqwJpVFhxbGiYq31+/piTSa7swhQpTlKioNnHplKrbn7oA==;")
                .BatchSize(32)
                .DegreeOfReceiveParallelism(32);

            configuration.UsePersistence<InMemoryPersistence>();
            configuration.UseSerialization<JsonSerializer>();
            configuration.LimitMessageProcessingConcurrencyTo(Constants.MaxConcurrency);
            configuration.EnableInstallers();

            var bus = Endpoint.Start(configuration).GetAwaiter().GetResult();
            stopWatch.Stop();

            Console.WriteLine($"Initalizing the bus took { stopWatch.Elapsed.ToString("G")}");
            stopWatch.Reset();
            stopWatch.Start();
            Syncher.SyncEvent.Wait();
            stopWatch.Stop();
            Console.WriteLine($"Receiving #{ Syncher.SyncEvent.InitialCount } of msgs over the bus took { stopWatch.Elapsed.ToString("G")}");

            Console.ReadLine();
        }
    }
}
