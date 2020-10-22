using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Acnys.Core.Request.Abstractions;
using Autofac.Features.Decorators;
using Serilog;

namespace Acnys.Core.Tracing
{
    //public class RequestSenderTracingBehavior : ISendRequest
    //{
    //    private readonly ILogger _log;
    //    private readonly TracingContext _tracingContext;
    //    private readonly ISendRequest _next;
    //    private readonly IDecoratorContext _context;

    //    public RequestSenderTracingBehavior(ILogger log, TracingContext tracingContext, ISendRequest next, IDecoratorContext context)
    //    {
    //        _log = log;
    //        _tracingContext = tracingContext;
    //        _next = next;
    //        _context = context;
    //    }

    //    public async Task Send<T>(T command, IDictionary<string, object> arguments = null, CancellationToken cancellationToken = default) where T : ICommand
    //    {
    //        arguments.UpdateWithTracingContext(_log, _tracingContext);
    //        await _next.Send(command, arguments, cancellationToken);
    //    }

    //    public async Task<T> Send<T>(IQuery<T> query, IDictionary<string, object> arguments = null, CancellationToken cancellationToken = default)
    //    {
    //        arguments.UpdateWithTracingContext(_log, _tracingContext);
    //        return await _next.Send(query, arguments, cancellationToken);
    //    }
    //}
}