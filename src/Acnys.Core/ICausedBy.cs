using System;

namespace Acnys.Core
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