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
        Shift = 0x01,

        /// <summary>
        /// The strategy multiplies and shifts.
        /// </summary>
        MultiplyShift = 0x10,

        /// <summary>
        /// The strategy multiplies, adds, and shifts.
        /// </summary>
        MultiplyAddShift = 0x11
    }
}
