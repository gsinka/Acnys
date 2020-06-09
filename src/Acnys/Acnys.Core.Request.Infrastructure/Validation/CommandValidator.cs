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
    public class CommandValidator<TCommand> : IHandleCommand<TCommand> where TCommand : ICommand
    {
        private readonly ILogger _log;
        private readonly IEnumerable<IValidator<TCommand>> _validators;
        private readonly IHandleCommand<TCommand> _commandHandler;

        public CommandValidator(ILogger log, IEnumerable<IValidator<TCommand>> validators, IHandleCommand<TCommand> commandHandler)
        {
            _log = log;
            _validators = validators;
            _commandHandler = commandHandler;
        }

        public async Task Handle(TCommand command, IDictionary<string, object> arguments = null, CancellationToken cancellationToken = default)
        {
            if (_validators.Any())
            {
                _log.Debug("Validating command {commandType}", command.GetType());

                foreach (var validator in _validators)
                {
                    _log.Verbose("Validating command {commandType} with {validatorType}", command.GetType(), validator.GetType());
                    await validator.ValidateAndThrowAsync(command, null, cancellationToken);
                }

                _log.Verbose("Validation of command {commandType} succeeded", command.GetType());
            }
            else
            {
                _log.Debug("No validator found for command {commandType}", command.GetType());
            }

            _log.Verbose("Handling {commandType} with {handler} ({handlerId})", typeof(TCommand), _commandHandler.GetType().Name, _commandHandler.GetHashCode());
            await _commandHandler.Handle(command, arguments, cancellationToken);
        }
    }
}
