using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;

namespace Acnys.Core.AspNet
{
    public static class CorrelationAspNetExtensions
    {
        public static IDictionary<string, object> ExtractArguments(this HttpRequest httpRequest)
        {
            return httpRequest.Headers.Keys.ToDictionary<string, string, object>(key => key, key => httpRequest.Headers[key]);
        }
    }
}