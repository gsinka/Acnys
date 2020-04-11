using System;

namespace Acnys.Core.Request.Tests
{
    public class TestCommand : Command
    {
        public TestCommand(Guid? requestId = null) : base(requestId)
        {
        }
    }
}