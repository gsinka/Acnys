using System;

namespace Acnys.Request.Abstractions
{
    public interface IRequest
    {
        Guid RequestId { get; }
    }
}