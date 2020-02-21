using System;
using System.Collections.Generic;
using System.Text;
using Acnys.Core.Abstractions.Extensions;
using Acnys.Core.Eventing.Abstractions;

namespace Acnys.Core.Eventing
{
    public static class EventingCorrelationExtensions
    {
        public static IDictionary<string, object> CorrelateTo(this IDictionary<string, object> source, IEvent @event)
        {
            return new Dictionary<string, object>{
                { CorrelationExtensions.CorrelationIdName, source.CorrelationId() },
                { CorrelationExtensions.CausationIdName, @event.EventId },
            };
        }
    }
}
