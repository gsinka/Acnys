using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Acnys.Core.Eventing;
using Microsoft.Extensions.ObjectPool;
using RabbitMQ.Client;

namespace Acnys.Core.Hosting.RabbitMQ.Pooled
{
    public class RabbitService : IRabbitService
    {
        private readonly string _exchange;
        private readonly string _routingKey;
        private readonly DefaultObjectPool<IModel> _objectPool;

        public RabbitService(IPooledObjectPolicy<IModel> objectPolicy/*, string exchange, string routingKey*/)
        {
            //_exchange = exchange;
            //_routingKey = routingKey;
            _objectPool = new DefaultObjectPool<IModel>(objectPolicy, 2/*Environment.ProcessorCount * 2*/);
        }

        public Task Publish<T>(T evnt, CancellationToken cancellationToken = default) where T : IEvent
        {
            return Task.Run(() => _objectPool.Get().BasicPublish(_exchange, _routingKey), cancellationToken);
        }

        public async Task Publish<T>(IList<T> events, CancellationToken cancellationToken = default) where T : IEvent
        {
            foreach (var @event in events)
            {
                await Publish(@event, cancellationToken);
            }
        }

        public IModel Channel => _objectPool.Get();
    }
}