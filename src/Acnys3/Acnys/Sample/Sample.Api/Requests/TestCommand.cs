using System;
using Acnys.Core.Request;
using FluentValidation;

namespace Sample.Api.Requests
{
    public class TestCommand : Command
    {
        public string Data { get; }

        public TestCommand(string data, Guid? causationId = null, Guid? correlationId = null) : base(causationId, correlationId)
        {
            Data = data;
        }
    }

    public class TestCommandValidator : AbstractValidator<TestCommand>
    {
        public TestCommandValidator()
        {
            RuleFor(x => x.Data).NotEmpty().WithMessage("Data cannot be empty");
        }
    }

}
