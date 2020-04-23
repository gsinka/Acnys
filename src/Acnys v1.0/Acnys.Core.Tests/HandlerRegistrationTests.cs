using System;
using System.Collections.Generic;
using System.Text;
using Acnys.Core.Application.Abstractions;
using Acnys.Core.Infrastructure;
using Api;
using Application;
using Autofac;
using Autofac.Core;
using Xunit;

namespace Acnys.Core.Tests
{
    public class HandlerRegistrationTests
    {
        [Fact]
        public void Single_command_handler_registration()
        {
            var builder = new ContainerBuilder();
            builder.RegisterCommandHandler<SampleCommandHandler>();
            var container = builder.Build();

            Assert.True(container.IsRegistered<IHandleCommand<SampleCommand>>());
        }

        [Fact]
        public void Single_command_handler_registration_with_wrong_type()
        {
            var builder = new ContainerBuilder();
            Assert.Throws<InvalidOperationException>(() => builder.RegisterCommandHandler<SampleQueryHandler>());
        }

        [Fact]
        public void Command_handler_registration_from_assembly()
        {
            var builder = new ContainerBuilder();
            builder.RegisterCommandHandlersFromAssemblyOf<SampleCommandHandler>();
            var container = builder.Build();

            Assert.True(container.IsRegistered<IHandleCommand<SampleCommand>>());
        }

        [Fact]
        public void Single_query_handler_registration()
        {
            var builder = new ContainerBuilder();
            builder.RegisterQueryHandler<SampleQueryHandler>();
            var container = builder.Build();

            Assert.True(container.IsRegistered<IHandleQuery<SampleQuery, SampleQuery.Result>>());
        }

        [Fact]
        public void Single_query_handler_registration_with_wrong_type()
        {
            var builder = new ContainerBuilder();
            Assert.Throws<InvalidOperationException>(() => builder.RegisterQueryHandler<SampleCommandHandler>());
        }

        [Fact]
        public void Query_handler_registration_from_assembly()
        {
            var builder = new ContainerBuilder();
            builder.RegisterQueryHandlersFromAssemblyOf<SampleQueryHandler>();
            var container = builder.Build();

            Assert.True(container.IsRegistered<IHandleQuery<SampleQuery, SampleQuery.Result>>());
        }


    }
}
