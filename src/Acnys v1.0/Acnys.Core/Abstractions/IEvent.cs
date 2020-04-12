using System;

namespace Acnys.Core.Abstractions
{
    /// <summary>
    /// Event interface
    /// </summary>
    public interface IEvent
    {
        /// <summary>
        /// Unique event identifier
        /// </summary>
        Guid EventId { get; }
    }
}