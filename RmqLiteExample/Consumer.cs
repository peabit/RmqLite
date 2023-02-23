using RmqLite;

namespace RmqLiteExample
{
    public class Consumer : IConsumer<Message>
    {
        private readonly ILogger _logger;

        public Consumer(ILogger<Consumer> logger)
            => _logger = logger;

        public async Task Consume(Message message)
            => _logger.LogInformation(message.Value);
    }
}