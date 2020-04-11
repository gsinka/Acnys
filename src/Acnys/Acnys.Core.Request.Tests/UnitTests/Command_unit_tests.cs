using System;
using Xunit;

// ReSharper disable InconsistentNaming

namespace Acnys.Core.Request.Tests.UnitTests
{
    public class Command_unit_tests
    {
        [Fact]
        public void Request_id_filled_if_not_given()
        {
            var command = new TestCommand();
            Assert.NotEqual(Guid.Empty, command.RequestId);
        }

        [Fact]
        public void Request_id_equals_to_given()
        {
            var id = Guid.NewGuid();
            var command = new TestCommand(id);
            Assert.Equal(id, command.RequestId);
        }
    }
}
