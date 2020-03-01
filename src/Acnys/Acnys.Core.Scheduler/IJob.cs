using System;
using Acnys.Core.ValueObjects;

namespace Acnys.Core.Scheduler
{
    public interface IJob
    {
        Guid JobId { get; }
        DateTime RunOn { get; }
    }
}