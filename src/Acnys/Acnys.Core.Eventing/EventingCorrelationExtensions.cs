namespace Acnys.Core.Eventing
{
    public static class EventingCorrelationExtensions
    {
        //public static IDictionary<string, object> CorrelateTo(this IDictionary<string, object> source, IEvent @event)
        //{
        //    return new Dictionary<string, object>{
        //        { RequestConstants.CorrelationId, source.CorrelationId() },
        //        { RequestConstants.CausationId, @event.EventId },
        //        { RequestConstants.TraceId, source.TraceId()}
        //    };
        //}

        //public static IDictionary<string, object> EnrichWithCorrelation(this IDictionary<string, object> target, IEvent @event, IDictionary<string, object> source)
        //{
        //    target ??= new Dictionary<string, object>();

        //    var correlationId = source.CorrelationId();
        //    var traceId = source.TraceId();
        //    if (!string.IsNullOrWhiteSpace(traceId)) target.UseTraceId(traceId);
        //    if (correlationId.HasValue) target.UseCorrelationId(correlationId.Value);

        //    target.UseCausationId(@event.EventId);

        //    return target;
        //}

        //public static IDictionary<string, object> CreateCorrelationFromSource(this IDictionary<string, object> source, IEvent @event)
        //{
        //    var target = new Dictionary<string, object>();

        //    var correlationId = source.CorrelationId();
        //    var traceId = source.TraceId();
        //    if (!string.IsNullOrWhiteSpace(traceId)) target.UseTraceId(traceId);
        //    if (correlationId.HasValue) target.UseCorrelationId(correlationId.Value);

        //    target.UseCausationId(@event.EventId);

        //    return target;
        //}
    }
}
