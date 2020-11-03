using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Acnys.Request.Abstractions;
using Acnys.Request.Dispatchers;
using Autofac.Features.Decorators;

namespace Acnys.Correlation
{
    public class CorrelationBehavior : IDispatchCommand
    {
        private readonly IDispatchCommand _nextHandler;
        private readonly IDecoratorContext _decoratorContext;

        public CorrelationBehavior(IDispatchCommand nextHandler, IDecoratorContext decoratorContext)
        {
            _nextHandler = nextHandler;
            _decoratorContext = decoratorContext;
        }

        public async Task DispatchAsync<T>(T command, IDictionary<string, object> arguments = null, CancellationToken cancellationToken = default) where T : ICommand
        {

            await _nextHandler.DispatchAsync(command, arguments, cancellationToken);
        }
    }
}
