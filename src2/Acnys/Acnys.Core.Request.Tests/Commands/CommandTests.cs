using System;
using Xunit;

namespace Acnys.Core.Request.Tests.Commands
{
    public class RequestIdTests
    {
        [Fact]
        public void Command_base_id_check()
        {
            var command = new TestCommand();
            Assert.NotEqual(Guid.Empty, command.RequestId);
            Assert.Null(command.CausationId);
            Assert.Null(command.CorrelationId);

            var requestId = Guid.NewGuid();
            var correlationId = Guid.NewGuid();
            var causationId = Guid.NewGuid();
            
            command = new TestCommand(requestId, causationId, correlationId);
            Assert.Equal(requestId, command.RequestId);
            Assert.Equal(causationId, command.CausationId);
            Assert.Equal(correlationId, command.CorrelationId);
        }

        [Fact]
        public void Query_base_id_check()
        {
            var requestId = Guid.NewGuid();
            
            var query = new TestQuery();
            Assert.NotEqual(Guid.Empty, query.RequestId);

            query = new TestQuery(requestId);
            Assert.Equal(requestId, query.RequestId);
        }


        public class TestCommand : Command
        {
            public TestCommand(Guid? requestId = null, Guid? causationId = null, Guid? correlationId = null) : base(requestId, causationId, correlationId)
            {
            }
        }

        public class TestQuery : Query<string>
        {
            public TestQuery()
            {
            }

            public TestQuery(Guid requestId) : base(requestId)
            {
            }
        }
    }
}
