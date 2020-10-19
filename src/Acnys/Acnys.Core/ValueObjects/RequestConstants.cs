using Microsoft.Extensions.Configuration;

namespace Acnys.Core.ValueObjects
{
    public class RequestConstants
    {
        public const string CorrelationId = "correlation-id";
        public const string CausationId = "causation-id";
        public const string CausationPath = "causation-path";
        public const string RequestId = "request-id";
        public const string DomainType = "domain-type";
        public const string TraceId = "uber-trace-id";
    }
}
