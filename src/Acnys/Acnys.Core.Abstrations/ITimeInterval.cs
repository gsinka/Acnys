using System;

namespace Acnys.Core.Abstractions
{
    /// <summary>
    /// Time interval interface
    /// </summary>
    public interface ITimeInterval
    {
        DateTime Start { get; }
        DateTime Finish { get; }
        void Deconstruct(out DateTime start, out DateTime finish);
    }
}