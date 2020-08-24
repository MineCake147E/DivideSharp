using System;
using System.Collections.Generic;
using System.Text;

namespace DivideSharp
{
    /// <summary>
    /// Represents a strategy of division for <see cref="UInt32Divisor"/>.
    /// </summary>
    public enum UInt32DivisorStrategy : byte
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

    /// <summary>
    /// Represents a strategy of division for <see cref="Int32Divisor"/>.
    /// </summary>
    public enum Int32DivisorStrategy : byte
    {
        /// <summary>
        /// The strategy for dividing by 1.
        /// </summary>
        None = 0,

        /// <summary>
        /// The strategy for dividing by the number that is power of two.
        /// </summary>
        PowerOfTwoPositive = 0b10,

        /// <summary>
        /// The strategy for dividing by the negated number that is power of two.
        /// </summary>
        PowerOfTwoNegative = 0b11,

        /// <summary>
        /// The strategy multiplies and shifts.
        /// </summary>
        MultiplyShift = 0b100,

        /// <summary>
        /// The strategy multiplies, adds, and shifts.
        /// </summary>
        MultiplyAddShift = 0b110,

        /// <summary>
        /// The strategy multiplies, subtracts, and shifts.
        /// </summary>
        MultiplySubtractShift = 0b111,

        /// <summary>
        /// The strategy for MinValue that only branches.
        /// </summary>
        Branch = 0b1000,
    }
}
