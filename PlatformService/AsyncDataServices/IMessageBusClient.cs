using PlatformService.Dtos;
using Dyconits.Event;

namespace PlatformService.AsyncDataServices
{
    public interface IMessageBusClient
    {
        void PublishNewPlatform(DyconitsEvent evnt);
    }
}