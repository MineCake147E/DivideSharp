using System;
using System.Collections.Generic;
using System.Text;

namespace DivideSharp
{
    /// <summary>
    /// Defines a base infrastructure of a divisor.
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
        /// Calculates the quotient of two n-bit (un)signed integers (<paramref name="value"/> and <see cref="Divisor"/>) and the remainder.
        /// </summary>
        /// <param name="value">The dividend.</param>
        /// <param name="quotient">The quotient of the specified numbers.</param>
        /// <returns>The remainder.</returns>
        T DivRem(T value, out T quotient);
    }

    /// <summary>
    /// Defines a base infrastructure of an unsigned divisor.
    /// </summary>
    public interface IUnsignedDivisor<T> : IDivisor<T> where T : unmanaged
    {
        /// <summary>
        /// Returns the largest multiple of <see cref="IDivisor{T}.Divisor"/> less than or equal to the specified <paramref name="value"/>.
        /// </summary>
        /// <param name="value">The dividend.</param>
        /// <returns>The largest multiple of <see cref="IDivisor{T}.Divisor"/> less than or equal to the specified <paramref name="value"/>.</returns>
        T Floor(T value);

        /// <summary>
        /// Calculates the largest multiple of <see cref="IDivisor{T}.Divisor"/> less than or equal to the specified <paramref name="value"/> and the remainder.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="largestMultipleOfDivisor">The largest multiple of <see cref="IDivisor{T}.Divisor"/> less than or equal to the specified <paramref name="value"/>.</param>
        /// <returns>The remainder.</returns>
        T FloorRem(T value, out T largestMultipleOfDivisor);
    }

    /// <summary>
    /// Defines a base infrastructure of a signed divisor.
    /// </summary>
    public interface ISignedDivisor<T> : IDivisor<T> where T : unmanaged
    {
        /// <summary>
        /// Returns a multiple of <see cref="IDivisor{T}.Divisor"/> that has the largest absolute value less than the absolute value of the specified <paramref name="value"/>.
        /// </summary>
        /// <param name="value">The dividend.</param>
        /// <returns>A multiple of <see cref="IDivisor{T}.Divisor"/> that has the largest absolute value less than the absolute value of the specified <paramref name="value"/>.</returns>
        T AbsFloor(T value);

        /// <summary>
        /// Calculates a multiple of <see cref="IDivisor{T}.Divisor"/> that has the largest absolute value less than the absolute value of the specified <paramref name="value"/> and stores it in <paramref name="largestMultipleOfDivisor"/>, and Returns the difference between <paramref name="largestMultipleOfDivisor"/> and <paramref name="value"/>.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="largestMultipleOfDivisor">A multiple of <see cref="IDivisor{T}.Divisor"/> that has the largest absolute value less than the absolute value of the specified <paramref name="value"/>.</param>
        /// <returns>The difference between <paramref name="largestMultipleOfDivisor"/> and <paramref name="value"/>.</returns>
        T AbsFloorRem(T value, out T largestMultipleOfDivisor);
    }
}
