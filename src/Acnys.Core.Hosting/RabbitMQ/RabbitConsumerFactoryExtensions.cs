using System;
using System.Collections.Generic;
using Acnys.Core.Request.Application;
using Autofac;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

// ReSharper disable IdentifierTypo

namespace Acnys.Core.Hosting.RabbitMQ
{
    public static class RabbitConsumerFactoryExtensions
    {
        public delegate void MessageHandlerDelegate(EventingBasicConsumer consumer, BasicDeliverEventArgs args);

        public static ContainerBuilder AddRabbitConnection(this ContainerBuilder builder, IConnection connection)
        {
            builder.RegisterInstance(connection).AsImplementedInterfaces().SingleInstance();
            return builder;
        }

        public static ContainerBuilder AddRabbitConnection(this ContainerBuilder builder, Action<RabbitConnectionFactory> factory)
        {
            var con = new RabbitConnectionFactory();
            factory(con);
            builder.RegisterInstance(con.CreateConnection()).AsImplementedInterfaces().SingleInstance();
            return builder;
        }

        /// <summary>
        /// Use the given RabbitMQ connection for the consumer
        /// </summary>
        /// <param name="factory"></param>
        /// <param name="connection"></param>
        /// <returns></returns>
        public static RabbitConsumerFactory UseConnection(this RabbitConsumerFactory factory, Func<IConnection> connection)
        {
            factory.Connection = connection;
            return factory;
        }

        /// <summary>
        /// Create queue
        /// </summary>
        /// <param name="factory">Consumer factory instance</param>
        /// <param name="durable">Durable</param>
        /// <param name="exclusive">Exclusive</param>
        /// <param name="autoDelete">Auto delete</param>
        /// <param name="arguments">Queue arguments</param>
        /// <returns></returns>
        public static RabbitConsumerFactory CreateQueue(this RabbitConsumerFactory factory, bool durable = false, bool exclusive = true, bool autoDelete = true, Dictionary<string, object> arguments = null)
        {
            factory.RegisterChannelCallback((queue, channel) => channel.QueueDeclare(queue, durable, exclusive, autoDelete, arguments));
            return factory;
        }

        /// <summary>
        /// Handle incoming messages with the given message handler
        /// </summary>
        /// <param name="factory">Consumer factory instance</param>
        /// <param name="handler">Message handler delegate</param>
        /// <returns></returns>
        public static RabbitConsumerFactory HandleMessage(this RabbitConsumerFactory factory, MessageHandlerDelegate handler)
        {
            factory.RegisterConsumerCallback(consumer => consumer.Received += (sender, args) =>  handler((EventingBasicConsumer)sender, args));
            return factory;
        }

        /// <summary>
        /// Dispatch incoming event messages using the given event mapper and event dispatcher
        /// </summary>
        /// <param name="factory">Consumer factory instance</param>
        /// <param name="eventMapper">Event mapper</param>
        /// <param name="eventDispatcher">Event dispatcher</param>
        /// <returns></returns>
        public static RabbitConsumerFactory DispatchMessage(this RabbitConsumerFactory factory, IMapEvent eventMapper, IDispatchEvent eventDispatcher)
        {
            factory.RegisterConsumerCallback(c => c.Received += async (sender, args) =>
            {
                var consumer = sender as EventingBasicConsumer ?? throw new ArgumentNullException(nameof(sender));

                var evnt = eventMapper.ToEvent(args);

                if (evnt == null)
                {
                    consumer.Model.BasicNack(args.DeliveryTag, false, false);
                } 
                else
                {
                    await eventDispatcher.Dispatch(evnt);
                    consumer.Model.BasicAck(args.DeliveryTag, false);
                }
            });

            return factory;
        }
    }
}