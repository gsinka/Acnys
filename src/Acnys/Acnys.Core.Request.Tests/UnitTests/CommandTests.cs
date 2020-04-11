using System;
using Xunit;

namespace Acnys.Core.Request.Tests.UnitTests
{
    public class RequestIdTests
    {
        [Fact]
        public void Command_base_id_check()
        {
            var command = new TestCommand();
            Assert.NotEqual(Guid.Empty, command.RequestId);

            var requestId = Guid.NewGuid();
            
            command = new TestCommand(requestId);
            Assert.Equal(requestId, command.RequestId);
        }

        [Fact]
        public void Query_base_id_check()
        {
            var query = new TestQuery();
            Assert.NotEqual(Guid.Empty, query.RequestId);

            var requestId = Guid.NewGuid();
            query = new TestQuery(requestId);
            Assert.Equal(requestId, query.RequestId);
        }
    }
}
