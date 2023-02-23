using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RabbitMQ.Client;
using RmqLite.Interfaces;

namespace RmqLite;

public static class ServicesExtension
{
    public static void AddRmqLite(this IServiceCollection services, Action<ISubscriptionsConfigurator>? configureConsumers = null)
    {
        services.AddSingleton<IConnectionFactory>(new ConnectionFactory(){ DispatchConsumersAsync = true });
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