using System;
using Acnys.Core.Abstractions;

namespace Acnys.Core
{
    /// <summary>
    /// Computer clock implementation of <typeparamref <see cref="IClock"/>
    /// </summary>
    public class ComputerClock : IClock
    {
        /// <summary>
        /// Current date and time in UTC
        /// </summary>
        public DateTime UtcNow => DateTime.UtcNow;
    }
}