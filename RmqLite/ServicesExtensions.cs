using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RabbitMQ.Client;
using RmqLite.Interfaces;

namespace RmqLite;

public static class ServicesExtensions
{
    public static void AddRmqLite(
        this IServiceCollection services, 
        IConfiguration? configuration = null, 
        Action<ISubscriptionsConfigurator>? configureConsumers = null
    )
    {
        var connectionFactory = new ConnectionFactory { DispatchConsumersAsync = true };

        if (configuration is not null)
        {
            var rabbitMqConfig = configuration.GetSection("RabbitMQ");
            connectionFactory.HostName = rabbitMqConfig.GetSection("HostName").Value;
            connectionFactory.UserName = rabbitMqConfig.GetSection("UserName").Value;
            connectionFactory.Password = rabbitMqConfig.GetSection("Password").Value;
        }

        services.AddSingleton<IConnectionFactory>(connectionFactory);
        services.AddSingleton<IPersistentConnection, PersistentConnection>();
        services.AddTransient<IPublisher, Publisher>();

        if (configureConsumers is not null)
        {
            var subscriptionsConfigurator = new SubscriptionsConfigurator();
            configureConsumers(subscriptionsConfigurator);

            var subscriptionProvider = subscriptionsConfigurator.GetProvider();

            foreach (var consumerType in subscriptionProvider.ConsumerTypes)
            {
                services.AddTransient(consumerType);
            }

            services.AddHostedService<ConsumingService>(
                sp => ActivatorUtilities.CreateInstance<ConsumingService>(sp, subscriptionProvider)
            );
        }
    }
}