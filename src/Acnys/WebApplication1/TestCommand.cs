using System;
using Acnys.Core.Request;

namespace WebApplication1.Commands
{
    public class TestCommand : Command
    {
        public string Data { get; }

        public TestCommand(string data, Guid? requestId = null) : base(requestId)
        {
            Data = data;
        }
    }
    public class AnotherTestCommand : Command
    {
        public string Data { get; }

        public AnotherTestCommand(string data, Guid? requestId = null) : base(requestId)
        {
            Data = data;
        }
    }
}