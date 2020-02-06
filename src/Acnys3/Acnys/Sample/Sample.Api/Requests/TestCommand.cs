using System;
using Acnys.Core.Request;

namespace Sample.Api.Facade
{
    public class TestCommand : Command
    {
        public string Data { get; }

        public TestCommand(string data, Guid? causationId = null, Guid? correlationId = null) : base(causationId, correlationId)
        {
            Data = data;
        }
    }
}
