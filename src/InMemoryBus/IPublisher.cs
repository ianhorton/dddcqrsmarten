using System.Threading.Tasks;

namespace InMemoryBus
{
    public interface IPublisher
    {
        Task PublishAsync(Message message);
    }
}
