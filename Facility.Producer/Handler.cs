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
            Syncher.SyncEvent.Signal();
        }
    }
}