using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Acnys.Core.Request.Abstractions;
using Autofac.Features.Decorators;
using FluentValidation;
using Serilog;

namespace Acnys.Core.Request.Infrastructure.Validation
{
    public class QueryValidator<TQuery, TResult> : IHandleQuery<TQuery, TResult> where TQuery : IQuery<TResult>
    {
        private readonly ILogger _log;
        private readonly IEnumerable<IValidator<TQuery>> _validators;
        private readonly IHandleQuery<TQuery, TResult> _queryHandler;

        public QueryValidator(ILogger log, IEnumerable<IValidator<TQuery>> validators, IHandleQuery<TQuery, TResult> queryHandler)
        {
            _log = log;
            _validators = validators;
            _queryHandler = queryHandler;
        }

        public async Task<TResult> Handle(TQuery query, IDictionary<string, object> arguments = null, CancellationToken cancellationToken = default)
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

            return await _queryHandler.Handle(query, arguments, cancellationToken);
        }
    }
}