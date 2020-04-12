using System;
using System.Collections.Generic;
using Acnys.Core.Helper;
using Acnys.Core.ValueObjects;
using Xunit;

namespace Acnys.Core.Tests
{
    public class CausationTests
    {
        [Fact]
        public void Add_causationId_to_arguments()
        {
            var arguments = new Dictionary<string, object>();
            var causationId = Guid.NewGuid();

            arguments.UseCausationId(causationId);
            Assert.Contains(arguments,pair => pair.Key == RequestConstants.CausationId && pair.Value.Equals(causationId));
        }

        [Fact]
        public void Add_null_causationId_to_arguments()
        {
            var arguments = new Dictionary<string, object>();
            arguments.UseCausationId(null);
            Assert.Empty(arguments);
        }

        [Fact]
        public void Add_causationId_to_null_arguments_throws_exception()
        {
            Assert.Throws<ArgumentNullException>(() => ((IDictionary<string, object>)null).UseCausationId(Guid.NewGuid()));
        }

        [Fact]
        public void Get_causationId_from_null_arguments_gives_null()
        {
            Assert.Null(((IDictionary<string, object>)null).CausationId());
        }

        [Fact]
        public void Get_guid_causationId_from_arguments()
        {
            var arguments = new Dictionary<string, object>();
            var causationId = Guid.NewGuid();
            arguments.Add(RequestConstants.CausationId, causationId);

            Assert.Equal(causationId, arguments.CausationId());
        }

        [Fact]
        public void Get_string_causationId_from_arguments()
        {
            var arguments = new Dictionary<string, object>();
            var causationId = Guid.NewGuid();
            arguments.Add(RequestConstants.CausationId, causationId.ToString());

            Assert.Equal(causationId, arguments.CausationId());
        }

        [Fact]
        public void Get_invalid_causationId_from_arguments_gives_null()
        {
            var arguments = new Dictionary<string, object> {{RequestConstants.CausationId, "invalid_guid"}};
            Assert.Null(arguments.CausationId());
        }

        [Fact]
        public void CausedBy_for_command_updates_causationId_in_arguments()
        {
            var correlationId = Guid.NewGuid();

            var arguments = new Dictionary<string, object>();
            arguments.UseCorrelationId(correlationId);

            var command = new TestCommand();
            arguments.CausedBy(command);

            Assert.Equal(correlationId, arguments.CorrelationId());
            Assert.Equal(command.RequestId, arguments.CausationId());
        }

        [Fact]
        public void CausedBy_for_event_updates_causationId_in_arguments()
        {
            var correlationId = Guid.NewGuid();

            var arguments = new Dictionary<string, object>();
            arguments.UseCorrelationId(correlationId);

            var evnt = new TestEvent();
            arguments.CausedBy(evnt);

            Assert.Equal(correlationId, arguments.CorrelationId());
            Assert.Equal(evnt.EventId, arguments.CausationId());
        }

        private class TestCommand : Command {  }
        private class TestEvent : Event { }
    }
}