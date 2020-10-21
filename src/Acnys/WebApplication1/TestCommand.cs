using System;
using Acnys.Core.Request;
using Acnys.Core.Tracing.Attributes;

namespace WebApplication1.Commands
{
    [HumanReadableInformation("Test command","Test command's deteailed description")]
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