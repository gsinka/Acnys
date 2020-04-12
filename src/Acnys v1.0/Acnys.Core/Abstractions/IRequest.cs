using System;

namespace Acnys.Core.Abstractions
{
    public interface IRequest
    {
        Guid RequestId { get; }
    }
}