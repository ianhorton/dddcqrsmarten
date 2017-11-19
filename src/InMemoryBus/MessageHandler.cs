using System;
using System.Threading.Tasks;

namespace InMemoryBus
{
    internal interface IMessageHandler
    {
        string HandlerName { get; }
        Task<bool> TryHandleAsync(Message message);
        bool IsSame<T>(object handler);
    }

    internal class MessageHandler<T> : IMessageHandler where T : Message
    {
        public string HandlerName { get; private set; }

        private readonly IHandle<T> _handler;

        public MessageHandler(IHandle<T> handler, string handlerName)
        {
            if (handler == null)
                throw new ArgumentNullException("handler");
            _handler = handler;
            HandlerName = handlerName ?? string.Empty;
        }

        public async Task<bool> TryHandleAsync(Message message)
        {
            var msg = message as T;
            if (msg != null)
            {
                await _handler.HandleAsync(msg).ConfigureAwait(false);
                return true;
            }
            return false;
        }

        public bool IsSame<T2>(object handler)
        {
            return ReferenceEquals(_handler, handler) && typeof(T) == typeof(T2);
        }

        public override string ToString()
        {
            return string.IsNullOrEmpty(HandlerName) ? _handler.ToString() : HandlerName;
        }
    }
}