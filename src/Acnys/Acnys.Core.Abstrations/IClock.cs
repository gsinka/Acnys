using System;

namespace Acnys.Core.Abstractions
{
    /// <summary>
    /// IClock interface for getting actual date and time in UTC format
    /// </summary>
    public interface IClock
    {
        /// <summary>
        /// Current date and time in UTC
        /// </summary>
        DateTime UtcNow { get; }
    }
}