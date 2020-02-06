﻿using System.Threading;
using System.Threading.Tasks;
using Acnys.Core.Abstractions;

namespace Acnys.Core.Application.Abstractions
{
    /// <summary>
    /// Command sender interface
    /// </summary>
    /// <remarks>The command sender sends command to </remarks>
    public interface ISendCommand
    {
        Task Send<T>(T command,  CancellationToken cancellationToken = default) where T : ICommand;
    }
}