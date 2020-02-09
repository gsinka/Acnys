using System;
using System.Collections.Generic;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Acnys.Core.Hosting.RabbitMQ
{
    public class RabbitConsumerFactory
    {
        public Func<IConnection> Connection;
        
        private readonly IList<Action<string, IModel>> _channelCallbacks = new List<Action<string, IModel>>();
        private readonly IList<Action<EventingBasicConsumer>> _consumerCallbacks = new List<Action<EventingBasicConsumer>>();
        internal bool _autoAck;

        public void RegisterChannelCallback(Action<string, IModel> channelCallback)
        {
            _channelCallbacks.Add(channelCallback);
        }
        public void RegisterConsumerCallback(Action<EventingBasicConsumer> consumer)
        {
            _consumerCallbacks.Add(consumer);
        }

        public EventingBasicConsumer CreateConsumer(string queue, bool autoAck, string consumerTag = null, Dictionary<string, object> arguments = null)
        {
            _autoAck = autoAck;

            var channel = Connection().CreateModel();
            foreach (var action in _channelCallbacks) { action(queue, channel); }
            
            var consumer = new EventingBasicConsumer(channel);
            foreach (var action in _consumerCallbacks) { action(consumer); }
            
            var tag = channel.BasicConsume(queue, autoAck, consumerTag ?? string.Empty, arguments, consumer);

            return consumer;
        }
    }
}
