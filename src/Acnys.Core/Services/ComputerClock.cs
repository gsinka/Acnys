using System;

namespace Acnys.Core.Services
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