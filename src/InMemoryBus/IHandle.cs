using System.Threading.Tasks;

namespace InMemoryBus
{
    public interface IHandle<in T> where T : Message
    {
        Task HandleAsync(T message);
    }
}