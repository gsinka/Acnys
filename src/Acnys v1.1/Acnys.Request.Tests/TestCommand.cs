using System;
using Acnys.Request.Abstractions;

namespace Acnys.Request.Tests
{
    public class TestCommand : Command
    {
        public TestCommand(Guid? requestId = null) : base(requestId ?? Guid.NewGuid())
        {
        }
    }
}