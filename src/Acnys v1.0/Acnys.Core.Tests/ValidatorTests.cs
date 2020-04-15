using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Acnys.Core.Application.Abstractions;
using Acnys.Core.Infrastructure;
using Acnys.Core.Tests.Helpers;
using Autofac;
using FluentValidation;
using Serilog;
using Xunit;
using Xunit.Abstractions;

namespace Acnys.Core.Tests
{
    public class ValidatorTests
    {
        private readonly ISendCommand _commandSender;
        private readonly ISendQuery _querySender;
        
        public ValidatorTests(ITestOutputHelper testOutputHelper)
        {
            var builder = new ContainerBuilder();

            Log.Logger = new LoggerConfiguration().ConfigureLogForTesting(testOutputHelper).CreateLogger();
            builder.RegisterInstance(Log.Logger).As<ILogger>().SingleInstance();

            builder.RegisterLoopbackCommandSender();
            builder.RegisterLoopbackQuerySender();

            builder.AddCommandValidationBehaviour();
            builder.AddQueryValidationBehaviour();

            builder.RegisterCommandDispatcher();
            builder.RegisterQueryDispatcher();
            
            builder.RegisterValidator<TestCommandValidator>();
            builder.RegisterCommandHandler<TestCommandHandler>();

            builder.RegisterValidator<TestQueryValidator>();
            builder.RegisterCommandHandler<TestQueryHandler>();

            var container = builder.Build();
            
            _commandSender = container.Resolve<ISendCommand>();
            _querySender = container.Resolve<ISendQuery>();
        }

        [Fact]
        public async Task Test_command_validation_succeeds()
        {
            await _commandSender.Send(new TestCommand("has data"));
        }
        
        [Fact]
        public async Task Test_command_validation_succeeds_without_validator()
        {
            await _commandSender.Send(new TestCommandWithoutValidator());
        }

        [Fact]
        public async Task Test_command_validation_fails()
        {
            var exception = await Assert.ThrowsAsync<ValidationException>( async () => await _commandSender.Send(new TestCommand(string.Empty)));
            Assert.NotEmpty(exception.Message);
        }

        [Fact]
        public async Task Test_query_validation_succeeds()
        {
            var result = await _querySender.Send(new TestQuery("has data"));
            Assert.Equal("has data", result);
        }
        
        [Fact]
        public async Task Test_query_validation_succeeds_without_validator()
        {
            var result = await _querySender.Send(new TestQueryWithoutValidator(string.Empty));
            Assert.Equal(string.Empty, result);
        }

        [Fact]
        public async Task Test_query_validation_fails()
        {
            var exception = await Assert.ThrowsAsync<ValidationException>( async () => await _querySender.Send(new TestQuery(string.Empty)));
            Assert.NotEmpty(exception.Message);
        }

        public class TestCommand : Command
        {
            public string Data { get; }

            public TestCommand(string data, Guid? requestId = null) : base(requestId ?? Guid.NewGuid())
            {
                Data = data;
            }
        }
        
        public class TestCommandWithoutValidator : Command
        {
            public TestCommandWithoutValidator(Guid? requestId = null) : base(requestId ?? Guid.NewGuid()) { }
        }

        public class TestCommandValidator : AbstractValidator<TestCommand>
        {
            public TestCommandValidator()
            {
                RuleFor(x => x.Data).NotEmpty().WithErrorCode("1").WithMessage("message");
            }
        }

        public class TestCommandHandler : 
            IHandleCommand<TestCommand>, 
            IHandleCommand<TestCommandWithoutValidator>
        {
            public Task Handle(TestCommand command, IDictionary<string, object> arguments = null, CancellationToken cancellationToken = default)
            {
                return Task.CompletedTask;
            }

            public Task Handle(TestCommandWithoutValidator command, IDictionary<string, object> arguments = null, CancellationToken cancellationToken = default)
            {
                return Task.CompletedTask;
            }
        }

        public class TestQuery : Query<string>
        {
            public string Data { get; }

            public TestQuery(string data, Guid? requestId = null) : base(requestId ?? Guid.NewGuid())
            {
                Data = data;
            }
        }
        public class TestQueryWithoutValidator : Query<string>
        {
            public string Data { get; }

            public TestQueryWithoutValidator(string data, Guid? requestId = null) : base(requestId ?? Guid.NewGuid())
            {
                Data = data;
            }
        }

        public class TestQueryValidator : AbstractValidator<TestQuery>
        {
            public TestQueryValidator()
            {
                RuleFor(x => x.Data).NotEmpty().WithErrorCode("1").WithMessage("message");

            }
        }

        public class TestQueryHandler : IHandleQuery<TestQuery, string>, IHandleQuery<TestQueryWithoutValidator, string>
        {
            public Task<string> Handle(TestQuery query, IDictionary<string, object> arguments = null, CancellationToken cancellationToken = default)
            {
                return Task.FromResult(query.Data);
            }

            public Task<string> Handle(TestQueryWithoutValidator query, IDictionary<string, object> arguments = null, CancellationToken cancellationToken = default)
            {
                return Task.FromResult(query.Data);
            }
        }
    }
}
