using System;
using System.Collections.Generic;
using System.Text;

namespace DivideSharp
{
    /// <summary>
    /// Defines a base infrastructure of an Divisor.
    /// </summary>
    public interface IDivisor<T> where T : unmanaged
    {
        /// <summary>
        /// Gets the divisor.
        /// </summary>
        /// <value>
        /// The divisor.
        /// </value>
        T Divisor { get; }

        /// <summary>
        /// Divides the specified value by <see cref="Divisor"/>.
        /// </summary>
        /// <param name="value">The dividend.</param>
        /// <returns>The value divided by <see cref="Divisor"/>.</returns>
        T Divide(T value);

        /// <summary>
        /// Calculates the remainder by <see cref="Divisor"/> of the specified <paramref name="value"/>.
        /// </summary>
        /// <param name="value">The dividend.</param>
        /// <returns>The remainder.</returns>
        T Modulo(T value);

        /// <summary>
        /// Calculates the quotient of two 32-bit signed integers (<paramref name="value"/> and <see cref="Divisor"/>) and the remainder.
        /// </summary>
        /// <param name="value">The dividend.</param>
        /// <param name="quotient">The quotient of the specified numbers.</param>
        /// <returns>The remainder.</returns>
        T DivRem(T value, out T quotient);

        /// <summary>
        /// Returns the largest multiple of <see cref="Divisor"/> less than or equal to the specified <paramref name="value"/>.
        /// </summary>
        /// <param name="value">The dividend.</param>
        /// <returns>The largest multiple of <see cref="Divisor"/> less than or equal to the specified <paramref name="value"/>.</returns>
        T Floor(T value);

        /// <summary>
        /// Calculates the largest multiple of <see cref="Divisor"/> less than or equal to the specified <paramref name="value"/> and the remainder.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="largestMultipleOfDivisor">The largest multiple of <see cref="Divisor"/> less than or equal to the specified <paramref name="value"/>.</param>
        /// <returns>The remainder.</returns>
        T FloorRem(T value, out T largestMultipleOfDivisor);
    }
}
