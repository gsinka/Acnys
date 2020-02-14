using System;

namespace Acnys.Core.Request.Abstractions
{
    public interface IRequest
    {
        Guid RequestId { get; }
    }
}