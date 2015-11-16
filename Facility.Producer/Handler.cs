using System.Threading;
using System.Threading.Tasks;
using Messages;
using NServiceBus;

namespace Facility.Producer
{
    public class Handler : IHandleMessages<ProduceChocolateBar>
    {
        public async Task Handle(ProduceChocolateBar message, IMessageHandlerContext context)
        {
            await Task.Delay(100).ConfigureAwait(false);
            Syncher.SyncEvent.Signal();
        }
    }
}