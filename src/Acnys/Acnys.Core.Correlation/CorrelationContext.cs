using System;

namespace Acnys.Core.Correlation
{
    public class CorrelationContext
    {
        public Guid CorrelationId { get; internal set; }
        public Guid CausationId { get; internal set; }
    }
}
