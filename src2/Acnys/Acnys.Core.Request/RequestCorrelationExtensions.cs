using System.Collections.Generic;
using Acnys.Core.Abstractions.Extensions;
using Acnys.Core.Request.Abstractions;

namespace Acnys.Core.Request
{
    public static class RequestCorrelationExtensions
    {
        public static IDictionary<string, object> CorrelateTo(this IDictionary<string, object> source, ICommand command)
        {
            return new Dictionary<string, object>
            {
                { CorrelationExtensions.CorrelationIdName, source.CorrelationId() },
                { CorrelationExtensions.CausationIdName, command.RequestId }
            };
        }
    }
}