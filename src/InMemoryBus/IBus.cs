namespace InMemoryBus
{
    public interface IBus : IPublisher, ISubscriber
    {
        string Name { get; }
    }
}
