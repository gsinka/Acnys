using System.Collections.Generic;
using Acnys.Core.Abstractions.Extensions;
using Acnys.Core.Request.Abstractions;

namespace Acnys.Core.Request
{
    public static class RequestCorrelationExtensions
    {
        public static IDictionary<string, object> EnrichWithCorrelation(this IDictionary<string, object> target, ICommand command, IDictionary<string, object> source)
        {
            target ??= new Dictionary<string, object>();
            
            var correlationId = source.CorrelationId();
            if (correlationId.HasValue) target.UseCorrelationId(correlationId.Value);
            
            target.UseCausationId(command.RequestId);

            return target;
        }

        public static IDictionary<string, object> CreateCorrelationFromSource(this IDictionary<string, object> source, ICommand command)
        {
            var target = new Dictionary<string, object>();

            var correlationId = source.CorrelationId();
            if (correlationId.HasValue) target.UseCorrelationId(correlationId.Value);

            target.UseCausationId(command.RequestId);

            return target;
        }



    }
}