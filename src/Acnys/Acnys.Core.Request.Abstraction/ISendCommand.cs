﻿using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Acnys.Core.Request.Abstractions
{
    /// <summary>
    /// Command sender interface
    /// </summary>
    /// <remarks>The command sender sends command to </remarks>
    public interface ISendCommand
    {
        Task Send<T>(T command, IDictionary<string, object> arguments = null, CancellationToken cancellationToken = default) where T : ICommand;
    }
}