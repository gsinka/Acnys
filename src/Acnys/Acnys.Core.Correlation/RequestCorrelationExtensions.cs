using System.Collections.Generic;
using Acnys.Core.Request.Abstractions;

namespace Acnys.Core.Correlation
{
    public static class RequestCorrelationExtensions
    {
        //public static IDictionary<string, object> EnrichWithCorrelation(this IDictionary<string, object> target, ICommand command, IDictionary<string, object> source)
        //{
        //    target ??= new Dictionary<string, object>();
            
        //    var correlationId = source.CorrelationId();
        //    var traceId = source.TraceId();
        //    if (correlationId.HasValue) target.UseCorrelationId(correlationId.Value);
        //    if (!string.IsNullOrWhiteSpace(traceId)) target.UseTraceId(traceId);
            
        //    target.UseCausationId(command.RequestId);

        //    return target;
        //}

        //public static IDictionary<string, object> CreateCorrelationFromSource(this IDictionary<string, object> source, ICommand command)
        //{
        //    var target = new Dictionary<string, object>();
        //    var traceId = source.TraceId();

        //    var correlationId = source.CorrelationId();
        //    if (correlationId.HasValue) target.UseCorrelationId(correlationId.Value);
        //    if (!string.IsNullOrWhiteSpace(traceId)) target.UseTraceId(traceId);

        //    target.UseCausationId(command.RequestId);

        //    return target;
        //}



    }
}