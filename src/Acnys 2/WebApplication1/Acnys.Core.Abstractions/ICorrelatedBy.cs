using System;

namespace Acnys.Core.Abstractions
{
    /// <summary>
    /// Correlation
    /// </summary>
    public interface ICorrelatedBy
    {
        /// <summary>
        /// Correlation id
        /// </summary>
        Guid? CorrelationId { get; }
    }
}