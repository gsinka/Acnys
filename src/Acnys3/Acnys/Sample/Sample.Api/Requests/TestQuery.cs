using Acnys.Core.Request;
using FluentValidation;

namespace Sample.Api.Requests
{
    public class TestQuery : Query<string>
    {
        public string Data { get; }

        public TestQuery(string data)
        {
            Data = data;
        }
    }

    public class TestQueryValidator : AbstractValidator<TestQuery>
    {
        public TestQueryValidator()
        {
            RuleFor(x => x.Data).NotEmpty().WithMessage("Data cannot be empty");
        }
    }
}