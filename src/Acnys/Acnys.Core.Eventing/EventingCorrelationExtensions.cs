using System.Collections.Generic;
using Acnys.Core.Eventing.Abstractions;
using Acnys.Core.Extensions;
using Acnys.Core.ValueObjects;

namespace Acnys.Core.Eventing
{
    public static class EventingCorrelationExtensions
    {
        public static IDictionary<string, object> CorrelateTo(this IDictionary<string, object> source, IEvent @event)
        {
            return new Dictionary<string, object>{
                { RequestConstants.CorrelationId, source.CorrelationId() },
                { RequestConstants.CausationId, @event.EventId },
            };
        }

        public static IDictionary<string, object> EnrichWithCorrelation(this IDictionary<string, object> target, IEvent @event, IDictionary<string, object> source)
        {
            target ??= new Dictionary<string, object>();

            var correlationId = source.CorrelationId();
            if (correlationId.HasValue) target.UseCorrelationId(correlationId.Value);

            target.UseCausationId(@event.EventId);

            return target;
        }

        public static IDictionary<string, object> CreateCorrelationFromSource(this IDictionary<string, object> source, IEvent @event)
        {
            var target = new Dictionary<string, object>();

            var correlationId = source.CorrelationId();
            if (correlationId.HasValue) target.UseCorrelationId(correlationId.Value);

            target.UseCausationId(@event.EventId);

            return target;
        }
    }
}
