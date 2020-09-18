using System;
using System.Collections.Generic;
using System.Linq;

using Newtonsoft.Json;

using NUnit.Framework;

using TestAndBenchmarkUtils;

namespace DivideSharp.Tests
{
    [TestFixture]
    public class Int16DivisionTests
    {
        #region Test Cases

        private static IEnumerable<short> Divisors
        {
            get
            {
                var divisors = new short[]
                {
                    //Obvious
                    1,
                    //Pre-Defined
                    3,
                    4,
                    5,
                    6,
                    7,
                    8,
                    9,
                    10,
                    11,
                    12,
                    //Shift
                    2,
                    16,
                    21, //MultiplyAdd
                    25 //Multiply
                };
                return divisors.Concat(divisors.Select(a => (short)-a)).Distinct().OrderBy(a => Math.Abs(a));
            }
        }

        private static IEnumerable<TestCaseData> RandomDivisionTestCaseSource => Divisors.Append(short.MinValue).Select(a => new TestCaseData(a));

        private const ulong RandomTestCount = ushort.MaxValue + 1;

        private static IEnumerable<TestCaseData> DivisionTestCaseSource
        {
            get
            {
                var divisors = Divisors;
                //Generate Test Data's
                foreach (var item in divisors)
                {
                    yield return new TestCaseData(item, (short)1);
                    yield return new TestCaseData(item, (short)(item - 1));
                    yield return new TestCaseData(item, item);
                    yield return new TestCaseData(item, (short)(item + 1));
                    yield return new TestCaseData(item, (short)(-item - 1));
                    yield return new TestCaseData(item, (short)-item);
                    yield return new TestCaseData(item, (short)(-item + 1));
                }
                //Branch
                yield return new TestCaseData(short.MinValue, (short)1);
                yield return new TestCaseData(short.MinValue, short.MinValue);
                yield return new TestCaseData(short.MinValue, (short)-short.MaxValue);
                yield return new TestCaseData(short.MinValue, (short)0);
                yield return new TestCaseData(short.MinValue, short.MaxValue);
            }
        }

        #endregion Test Cases

        private static string SerializeDivisor(Int16Divisor divisor) => $"{JsonConvert.SerializeObject(divisor)}";

        [TestCaseSource(nameof(DivisionTestCaseSource))]
        public void CorrectlyDivides(short divisor, short testDividend)
        {
            var int16Divisor = new Int16Divisor(divisor);
            var quotient = testDividend / int16Divisor;
            Console.WriteLine($"quotient:{quotient}");
            Assert.AreEqual(unchecked((short)(testDividend / divisor)), quotient, SerializeDivisor(int16Divisor));
        }

        [TestCaseSource(nameof(DivisionTestCaseSource))]
        public void CalculatesModulusCorrectly(short divisor, short testDividend)
        {
            var int16Divisor = new Int16Divisor(divisor);
            var remainder = testDividend % int16Divisor;
            Console.WriteLine($"remainder:{remainder}");
            Assert.AreEqual(unchecked((short)(testDividend % divisor)), remainder, SerializeDivisor(int16Divisor));
        }

        [TestCaseSource(nameof(DivisionTestCaseSource))]
        public void DivRemReturnsCorrectly(short divisor, short testDividend)
        {
            var int16Divisor = new Int16Divisor(divisor);
            var remainder = int16Divisor.DivRem(testDividend, out var quotient);
            Console.WriteLine($"quotient:{quotient}, remainder:{remainder}");
            Assert.Multiple(() =>
            {
                Assert.AreEqual(unchecked((short)(testDividend % divisor)), remainder, SerializeDivisor(int16Divisor));
                Assert.AreEqual(unchecked((short)(testDividend / divisor)), quotient, SerializeDivisor(int16Divisor));
            });
        }

        [TestCaseSource(nameof(DivisionTestCaseSource))]
        public void CalculatesAbsFloorCorrectly(short divisor, short testDividend)
        {
            var int16Divisor = new Int16Divisor(divisor);
            var rounded = int16Divisor.AbsFloor(testDividend);
            Console.WriteLine($"rounded:{rounded}");
            Assert.AreEqual((short)(unchecked((short)(testDividend / divisor)) * divisor), rounded, SerializeDivisor(int16Divisor));
        }

        [TestCaseSource(nameof(DivisionTestCaseSource))]
        public void CalculatesAbsFloorRemCorrectly(short divisor, short testDividend)
        {
            var int16Divisor = new Int16Divisor(divisor);
            var remainder = int16Divisor.AbsFloorRem(testDividend, out var rounded);
            Console.WriteLine($"rounded:{rounded}, remainder:{remainder}");
            Assert.Multiple(() =>
            {
                Assert.AreEqual(unchecked((short)(testDividend % divisor)), remainder, SerializeDivisor(int16Divisor));
                Assert.AreEqual((short)(unchecked((short)(testDividend / divisor)) * divisor), rounded, SerializeDivisor(int16Divisor));
            });
        }

        [TestCaseSource(nameof(RandomDivisionTestCaseSource))]
        public void CorrectlyDividesAllNumerators(short divisor)
        {
            var int16Divisor = new Int16Divisor(divisor);
            for (ulong i = 0; i < RandomTestCount; i++)
            {
                var testDividend = unchecked((short)i);
                var quotient = testDividend / int16Divisor;
                Assert.AreEqual(unchecked((short)(testDividend / divisor)), quotient, $"Trying to test {testDividend} / {divisor}");
            }
        }

        [TestCaseSource(nameof(RandomDivisionTestCaseSource))]
        public void CalculatesModulusCorrectlyAllNumerators(short divisor)
        {
            var int16Divisor = new Int16Divisor(divisor);
            for (ulong i = 0; i < RandomTestCount; i++)
            {
                var testDividend = unchecked((short)i);
                var remainder = testDividend % int16Divisor;
                Assert.AreEqual(unchecked((short)(testDividend % divisor)), remainder, $"Trying to test {testDividend} % {divisor}");
            }
        }

        [TestCaseSource(nameof(RandomDivisionTestCaseSource))]
        public void DivRemReturnsCorrectlyAllNumerators(short divisor)
        {
            var int16Divisor = new Int16Divisor(divisor);
            for (ulong i = 0; i < RandomTestCount; i++)
            {
                var testDividend = unchecked((short)i);
                var remainder = int16Divisor.DivRem(testDividend, out var quotient);
                Assert.AreEqual(unchecked((short)(testDividend % divisor)), remainder, $"Trying to test {testDividend} % {divisor}");
                Assert.AreEqual(unchecked((short)(testDividend / divisor)), quotient, $"Trying to test {testDividend} / {divisor}");
            }
        }

        [TestCaseSource(nameof(RandomDivisionTestCaseSource))]
        public void CalculatesAbsFloorCorrectlyAllNumerators(short divisor)
        {
            var int16Divisor = new Int16Divisor(divisor);
            for (ulong i = 0; i < RandomTestCount; i++)
            {
                var testDividend = unchecked((short)i);
                var rounded = int16Divisor.AbsFloor(testDividend);
                Assert.AreEqual((short)(unchecked((short)(testDividend / divisor)) * divisor), rounded, $"Trying to test {testDividend} / {divisor} * {divisor}");
            }
        }

        [TestCaseSource(nameof(RandomDivisionTestCaseSource))]
        public void CalculatesAbsFloorRemCorrectlyAllNumerators(short divisor)
        {
            var int16Divisor = new Int16Divisor(divisor);
            for (ulong i = 0; i < RandomTestCount; i++)
            {
                var testDividend = unchecked((short)i);
                var remainder = int16Divisor.AbsFloorRem(testDividend, out var rounded);
                Assert.AreEqual(unchecked((short)(testDividend % divisor)), remainder, $"Trying to test {testDividend} % {divisor}");
                Assert.AreEqual((short)(unchecked((short)(testDividend / divisor)) * divisor), rounded, $"Trying to test {testDividend} / {divisor} * {divisor}");
            }
        }
    }
}
