using System;
using Acnys.Core.Request;

namespace WebApplication1
{
    public class TestCommand : Command
    {
        public string Data { get; }

        public TestCommand(string data, Guid? requestId = null) : base(requestId)
        {
            Data = data;
        }
    }
}