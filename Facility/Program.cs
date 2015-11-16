using System;
using System.Collections.Generic;
using System.Diagnostics;
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
            var stopWatch = new Stopwatch();
            stopWatch.Start();
            DefaultFactory defaultFactory = LogManager.Use<DefaultFactory>();
            defaultFactory.Level(LogLevel.Error);

            var configuration = new BusConfiguration();
            configuration.EndpointName("Chocolate.Facility");

            configuration.UseTransport<MsmqTransport>();
            configuration.UsePersistence<InMemoryPersistence>();

            var bus = Endpoint.StartAsync(configuration).GetAwaiter().GetResult().CreateBusContext();
            stopWatch.Stop();

            Console.WriteLine($"Initalizing the bus took { stopWatch.Elapsed.ToString("G")}");
            stopWatch.Reset();

            var destination = "Chocolate.Facility.Producer";
            var tasks = new List<Task>();
            stopWatch.Start();
            for (int i = 0; i < Constants.NumberOfMessages; i++)
            {
                tasks.Add(bus.SendAsync(destination, new ProduceChocolateBar { LotNumber = i, MaxLotNumber = Constants.NumberOfMessages }));
            }
            Task.WhenAll(tasks).GetAwaiter().GetResult();
            stopWatch.Stop();
            Console.WriteLine($"Sending #{ Constants.NumberOfMessages } of msgs over the bus took { stopWatch.Elapsed.ToString("G")}");

            Console.ReadLine();
        }
    }
}
