using System;

namespace Acnys.Core.Abstractions
{
    /// <summary>
    /// Causation
    /// </summary>
    public interface ICausedBy
    {
        /// <summary>
        /// Causation id
        /// </summary>
        Guid? CausationId { get; }
    }
}