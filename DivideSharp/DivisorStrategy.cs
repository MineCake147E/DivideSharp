using System;
using System.Collections.Generic;
using System.Text;

namespace DivideSharp
{
    /// <summary>
    /// Represents a strategy of division.
    /// </summary>
    public enum DivisorStrategy : uint
    {
        /// <summary>
        /// The strategy for dividing by 1.
        /// </summary>
        None = 0,

        /// <summary>
        /// The strategy that only shifts.
        /// </summary>
        Shift = 0b01,

        /// <summary>
        /// The strategy multiplies and shifts.
        /// </summary>
        MultiplyShift = 0b10,

        /// <summary>
        /// The strategy multiplies, adds, and shifts.
        /// </summary>
        MultiplyAddShift = 0b11,

        /// <summary>
        /// The strategy for >MaxValue/2 that only branches.
        /// </summary>
        Branch = 0b100
    }
}
