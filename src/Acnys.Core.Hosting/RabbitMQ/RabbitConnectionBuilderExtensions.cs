using System;
using System.Collections.Generic;

namespace Acnys.Core.Hosting.RabbitMQ
{
    public static class RabbitConnectionFactoryExtensions
    {
        public static RabbitConnectionFactory UseUri(this RabbitConnectionFactory builder, string uri)
        {
            builder.RegisterConfigurationCallback(configuration => configuration.Uri = new Uri(uri));
            return builder;
        }

        public static RabbitConnectionFactory EnableAutoRecovery(this RabbitConnectionFactory builder, TimeSpan timespan = default)
        {
            builder.RegisterConfigurationCallback(configuration =>
            {
                configuration.AutomaticRecoveryEnabled = true;
                
                if (timespan != default)
                    configuration.NetworkRecoveryInterval = timespan;
            });

            return builder;
        }

        public static RabbitConnectionFactory UseClientProperties(this RabbitConnectionFactory builder, Dictionary<string, object> clientProperties)
        {
            builder.RegisterConfigurationCallback(configuration => configuration.ClientProperties = clientProperties);
            return builder;
        }

        public static RabbitConnectionFactory AddClientProperties(this RabbitConnectionFactory builder, Dictionary<string, object> clientProperties)
        {
            builder.RegisterConfigurationCallback(configuration =>
            {
                foreach (var property in clientProperties) { configuration.ClientProperties.Add(property); }
            });
            return builder;
        }


        public static RabbitConnectionFactory ClearClientProperties(this RabbitConnectionFactory builder)
        {
            builder.RegisterConfigurationCallback(configuration => configuration.ClientProperties.Clear());
            return builder;
        }

        public static RabbitConnectionFactory AddClientProperty(this RabbitConnectionFactory builder, string key, object value)
        {
            builder.RegisterConfigurationCallback(configuration => configuration.ClientProperties.Add(new KeyValuePair<string, object>(key, value)));
            return builder;
        }

        public static RabbitConnectionFactory OnRecoverySucceeded(this RabbitConnectionFactory builder, Action<object, EventArgs> recoveryAction)
        {
            builder.RegisterBuildCallback(configuration => configuration.RecoverySucceeded += (sender, args) =>  recoveryAction(sender, args));
            return builder;
        }

        public static RabbitConnectionFactory OnShutdown(this RabbitConnectionFactory builder, Action<object, EventArgs> shutdownAction)
        {
            builder.RegisterBuildCallback(configuration => configuration.ConnectionShutdown += (sender, args) => shutdownAction(sender, args));
            return builder;
        }
    }
}