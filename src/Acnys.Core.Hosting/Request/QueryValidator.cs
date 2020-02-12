using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Acnys.Core.Request;
using Acnys.Core.Request.ReadModel;
using Autofac.Features.Decorators;
using FluentValidation;
using Serilog;

namespace Acnys.Core.Hosting.Request
{
    public class QueryValidator<TQuery, TResult> : IHandleQuery<TQuery, TResult> where TQuery : IQuery<TResult>
    {
        private readonly ILogger _log;
        private readonly IEnumerable<IValidator<TQuery>> _validators;
        private readonly IHandleQuery<TQuery, TResult> _queryHandler;
        private readonly IDecoratorContext _decoratorContext;

        public QueryValidator(ILogger log, IEnumerable<IValidator<TQuery>> validators, IHandleQuery<TQuery, TResult> queryHandler, IDecoratorContext decoratorContext)
        {
            _log = log;
            _validators = validators;
            _queryHandler = queryHandler;
            _decoratorContext = decoratorContext;
        }

        public async Task<TResult> Handle(TQuery query, CancellationToken cancellationToken)
        {
            if (_validators.Any())
            {
                _log.Debug("Validating query {queryType}", query.GetType());

                foreach (var validator in _validators)
                {
                    _log.Verbose("Validating query {queryType} with {validatorType}", query.GetType(), validator.GetType());
                    await validator.ValidateAndThrowAsync(query, null, cancellationToken);
                }

                _log.Verbose("Validation of query {queryType} succeeded", query.GetType());
            }
            else
            {
                _log.Debug("No validator found for query {queryType}", query.GetType());
            }

            return await _queryHandler.Handle(query, cancellationToken);
        }
    }
}