﻿using System.Threading;
using System.Threading.Tasks;

namespace Acnys.Core.Request.Application
{
    /// <summary>
    /// Query sender interface
    /// </summary>
    /// <remarks>The query sender sends query to</remarks>
    public interface ISendQuery
    {
        Task<T> Send<T>(IQuery<T> query,  CancellationToken cancellationToken = default);
    }
}