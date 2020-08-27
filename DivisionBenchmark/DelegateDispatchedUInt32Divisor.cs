using System;
using System.Collections.Generic;
using System.Text;
using DivideSharp;

namespace DivisionBenchmark
{
    public readonly struct DelegateDispatchedUInt32Divisor : IDivisor<uint>
    {
        public DelegateDispatchedUInt32Divisor(uint divisor) : this()
        {
            Divisor = divisor;
            var w = new UInt32Divisor(divisor);
            Multiplier = w.Multiplier;
            Strategy = w.Strategy;
            Shift = w.Shift;
            switch (Strategy)
            {
                case UnsignedIntegerDivisorStrategy.Shift:
                    DivideFunc = ShiftOnly;
                    break;
                case UnsignedIntegerDivisorStrategy.MultiplyShift:
                    DivideFunc = Multiply;
                    break;
                case UnsignedIntegerDivisorStrategy.MultiplyAddShift:
                    DivideFunc = MultiplyAdd;
                    break;
                default:
                    DivideFunc = Echo;
                    break;
            }
        }

        /// <summary>
        /// Gets the divisor.
        /// </summary>
        /// <value>
        /// The divisor.
        /// </value>
        public uint Divisor { get; }

        /// <summary>
        /// Gets the multiplier for actual "division".
        /// </summary>
        /// <value>
        /// The multiplier.
        /// </value>
        public uint Multiplier { get; }

        /// <summary>
        /// Gets the strategy of a division.
        /// </summary>
        /// <value>
        /// The strategy of a division.
        /// </value>
        public UnsignedIntegerDivisorStrategy Strategy { get; }

        /// <summary>
        /// Gets the number of bits to shift for actual "division".
        /// </summary>
        /// <value>
        /// The number of bits to shift right.
        /// </value>
        public int Shift { get; }

        private Func<DelegateDispatchedUInt32Divisor, uint, uint> DivideFunc { get; }

        #region Divisions

#pragma warning disable IDE0060 // 未使用のパラメーターを削除します
#pragma warning disable S1172 // Unused method parameters should be removed

        private static uint Echo(DelegateDispatchedUInt32Divisor divisor, uint value) => value;

#pragma warning restore S1172 // Unused method parameters should be removed
#pragma warning restore IDE0060 // 未使用のパラメーターを削除します

        private static uint ShiftOnly(DelegateDispatchedUInt32Divisor divisor, uint value) => value >> divisor.Shift;

        private static uint Multiply(DelegateDispatchedUInt32Divisor divisor, uint value)
        {
            ulong rax = value;
            uint eax;
            ulong multiplier = divisor.Multiplier;
            int shift = divisor.Shift;
            rax *= multiplier;
            eax = (uint)(rax >> shift);
            return eax;
        }

        private static uint MultiplyAdd(DelegateDispatchedUInt32Divisor divisor, uint value)
        {
            ulong rax = value;
            uint eax;
            ulong multiplier = divisor.Multiplier;
            int shift = divisor.Shift;
            rax *= multiplier;
            eax = (uint)(rax >> 32);
            value -= eax;
            value >>= 1;
            eax += value;
            eax >>= shift;
            return eax;
        }

        #endregion Divisions

        public uint Divide(uint value) => DivideFunc(this, value);

        public uint DivRem(uint value, out uint quotient) => throw new NotImplementedException();

        public uint Floor(uint value) => throw new NotImplementedException();

        public uint FloorRem(uint value, out uint largestMultipleOfDivisor) => throw new NotImplementedException();

        public uint Modulo(uint value) => throw new NotImplementedException();
    }
}
