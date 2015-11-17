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
            stopWatch.Start();

            var tasks = new List<Task>();

            for (int i = 0; i < 8; i++)
            {
                var task = Task.Run(async () =>
                {
                    for (int j = 0; j < Constants.NumberOfMessages / 8; j++)
                    {
                        await bus.SendAsync(destination, new ProduceChocolateBar { LotNumber = j, MaxLotNumber = Constants.NumberOfMessages }).ConfigureAwait(false);
                    }
                });
                tasks.Add(task);
            }

            Task.WhenAll(tasks).GetAwaiter().GetResult();
            stopWatch.Stop();
            Console.WriteLine($"Sending #{ Constants.NumberOfMessages } of msgs over the bus took { stopWatch.Elapsed.ToString("G")}");

            Console.ReadLine();
        }
    }
}
