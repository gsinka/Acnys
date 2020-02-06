using System;

namespace Acnys.Core.Request
{
    /// <summary>
    /// Command interface
    /// </summary>
    public interface ICommand : IRequest
    {
        /// <summary>
        /// Unique id of command
        /// </summary>
        Guid Id { get; }
    }
}
