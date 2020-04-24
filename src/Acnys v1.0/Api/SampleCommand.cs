using System;
using Acnys.Core;
using FluentValidation;

namespace Api
{
    public class SampleCommand : Command
    {
        public Guid Id { get; }
        
        public SampleCommand(Guid id, Guid? requestId = null) : base(requestId ?? Guid.NewGuid())
        {
            Id = id;
        }
    }
}
