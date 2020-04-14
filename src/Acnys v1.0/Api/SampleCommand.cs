using System;
using Acnys.Core;

namespace Api
{
    public class SampleCommand : Command
    {
        public Guid Id { get; set; }

        public SampleCommand(Guid? requestId = null) : base(requestId ?? Guid.NewGuid())
        {
        }
    }
}
