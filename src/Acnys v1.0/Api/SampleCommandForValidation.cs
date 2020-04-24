using System;
using Acnys.Core;
using FluentValidation;

namespace Api
{
    public class SampleCommandForValidation : Command
    {
        public string Data { get; }

        public SampleCommandForValidation(string data, Guid? requestId = null) : base(requestId ?? Guid.NewGuid())
        {
            Data = data;
        }
    }

    public class SampleCommandForValidationValidator : AbstractValidator<SampleCommandForValidation>
    {
        public SampleCommandForValidationValidator()
        {
            RuleFor(x => x.Data).NotEmpty().WithMessage("Data cannot be empty");
        }

    }
}