using System;

namespace Acnys.Core.Abstractions
{
    /// <summary>
    /// Command interface
    /// </summary>
    public interface ICommand
    {
        /// <summary>
        /// Unique id of command
        /// </summary>
        Guid CommandId { get; }
    }
}
