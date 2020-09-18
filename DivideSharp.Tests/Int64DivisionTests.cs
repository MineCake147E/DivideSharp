using System;
using System.Collections.Generic;
using System.Linq;

using Newtonsoft.Json;

using NUnit.Framework;

using TestAndBenchmarkUtils;

namespace DivideSharp.Tests
{
    [TestFixture]
    public class Int64DivisionTests
    {
        #region Test Cases

        private static IEnumerable<long> Divisors
        {
            get
            {
                var divisors = new long[]
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
                    15, //MultiplyAdd or MultiplySub
                    29 //Multiply
                };
                return divisors.Concat(divisors.Select(a => -a)).Distinct().OrderBy(a => Math.Abs(a));
            }
        }

        private static IEnumerable<TestCaseData> RandomDivisionTestCaseSource => Divisors.Append(long.MinValue).Select(a => new TestCaseData(a));

        private const ulong RandomTestCount = 0x1_0000ul;

        private static IEnumerable<TestCaseData> DivisionTestCaseSource
        {
            get
            {
                var divisors = Divisors;
                //Generate Test Data's
                foreach (var item in divisors)
                {
                    yield return new TestCaseData(item, 1);
                    yield return new TestCaseData(item, item - 1);
                    yield return new TestCaseData(item, item);
                    yield return new TestCaseData(item, item + 1);
                    yield return new TestCaseData(item, -item - 1);
                    yield return new TestCaseData(item, -item);
                    yield return new TestCaseData(item, -item + 1);
                }
                //Branch
                yield return new TestCaseData(long.MinValue, 1);
                yield return new TestCaseData(long.MinValue, long.MinValue);
                yield return new TestCaseData(long.MinValue, -long.MaxValue);
                yield return new TestCaseData(long.MinValue, 0);
                yield return new TestCaseData(long.MinValue, long.MaxValue);
            }
        }

        #endregion Test Cases

        private static string SerializeDivisor(Int64Divisor divisor) => $"{JsonConvert.SerializeObject(divisor)}";

        [TestCaseSource(nameof(DivisionTestCaseSource))]
        public void CorrectlyDivides(long divisor, long testDividend)
        {
            var int64Divisor = new Int64Divisor(divisor);
            var quotient = testDividend / int64Divisor;
            Console.WriteLine($"quotient:{quotient}");
            Assert.AreEqual(testDividend / divisor, quotient, SerializeDivisor(int64Divisor));
        }

        [TestCaseSource(nameof(DivisionTestCaseSource))]
        public void CalculatesModulusCorrectly(long divisor, long testDividend)
        {
            var int64Divisor = new Int64Divisor(divisor);
            var remainder = testDividend % int64Divisor;
            Console.WriteLine($"remainder:{remainder}");
            Assert.AreEqual(testDividend % divisor, remainder, SerializeDivisor(int64Divisor));
        }

        [TestCaseSource(nameof(DivisionTestCaseSource))]
        public void DivRemReturnsCorrectly(long divisor, long testDividend)
        {
            var int64Divisor = new Int64Divisor(divisor);
            var remainder = int64Divisor.DivRem(testDividend, out var quotient);
            Console.WriteLine($"quotient:{quotient}, remainder:{remainder}");
            Assert.Multiple(() =>
            {
                Assert.AreEqual(testDividend % divisor, remainder, SerializeDivisor(int64Divisor));
                Assert.AreEqual(testDividend / divisor, quotient, SerializeDivisor(int64Divisor));
            });
        }

        [TestCaseSource(nameof(DivisionTestCaseSource))]
        public void CalculatesAbsFloorCorrectly(long divisor, long testDividend)
        {
            var int64Divisor = new Int64Divisor(divisor);
            var rounded = int64Divisor.AbsFloor(testDividend);
            Console.WriteLine($"rounded:{rounded}");
            Assert.AreEqual(testDividend / divisor * divisor, rounded, SerializeDivisor(int64Divisor));
        }

        [TestCaseSource(nameof(DivisionTestCaseSource))]
        public void CalculatesAbsFloorRemCorrectly(long divisor, long testDividend)
        {
            var int64Divisor = new Int64Divisor(divisor);
            var remainder = int64Divisor.AbsFloorRem(testDividend, out var rounded);
            Console.WriteLine($"rounded:{rounded}, remainder:{remainder}");
            Assert.Multiple(() =>
            {
                Assert.AreEqual(testDividend % divisor, remainder, SerializeDivisor(int64Divisor));
                Assert.AreEqual(testDividend / divisor * divisor, rounded, SerializeDivisor(int64Divisor));
            });
        }

        [TestCaseSource(nameof(RandomDivisionTestCaseSource))]
        public void CorrectlyDividesRandomNumerators(long divisor)
        {
            var int64Divisor = new Int64Divisor(divisor);
            var rng = new PcgRandom();
            for (ulong i = 0; i < RandomTestCount; i++)
            {
                var testDividend = unchecked(rng.Next() | ((long)rng.Next() << 32));
                var quotient = testDividend / int64Divisor;
                Assert.AreEqual(testDividend / divisor, quotient, $"Trying to test {testDividend} / {divisor}");
            }
        }

        [TestCaseSource(nameof(RandomDivisionTestCaseSource))]
        public void CalculatesModulusCorrectlyRandomNumerators(long divisor)
        {
            var int64Divisor = new Int64Divisor(divisor);
            var rng = new PcgRandom();

            for (ulong i = 0; i < RandomTestCount; i++)
            {
                var testDividend = unchecked(rng.Next() | ((long)rng.Next() << 32));
                var remainder = testDividend % int64Divisor;
                Assert.AreEqual(testDividend % divisor, remainder, $"Trying to test {testDividend} % {divisor}");
            }
        }

        [TestCaseSource(nameof(RandomDivisionTestCaseSource))]
        public void DivRemReturnsCorrectlyRandomNumerators(long divisor)
        {
            var int64Divisor = new Int64Divisor(divisor);
            var rng = new PcgRandom();
            for (ulong i = 0; i < RandomTestCount; i++)
            {
                var testDividend = unchecked(rng.Next() | ((long)rng.Next() << 32));
                var remainder = int64Divisor.DivRem(testDividend, out var quotient);
                Assert.AreEqual(testDividend % divisor, remainder, $"Trying to test {testDividend} % {divisor}");
                Assert.AreEqual(testDividend / divisor, quotient, $"Trying to test {testDividend} / {divisor}");
            }
        }

        [TestCaseSource(nameof(RandomDivisionTestCaseSource))]
        public void CalculatesAbsFloorCorrectlyRandomNumerators(long divisor)
        {
            var int64Divisor = new Int64Divisor(divisor);
            var rng = new PcgRandom();
            for (ulong i = 0; i < RandomTestCount; i++)
            {
                var testDividend = unchecked(rng.Next() | ((long)rng.Next() << 32));
                var rounded = int64Divisor.AbsFloor(testDividend);
                Assert.AreEqual(testDividend / divisor * divisor, rounded, $"Trying to test {testDividend} / {divisor} * {divisor}");
            }
        }

        [TestCaseSource(nameof(RandomDivisionTestCaseSource))]
        public void CalculatesAbsFloorRemCorrectlyRandomNumerators(long divisor)
        {
            var int64Divisor = new Int64Divisor(divisor);
            var rng = new PcgRandom();
            for (ulong i = 0; i < RandomTestCount; i++)
            {
                var testDividend = unchecked(rng.Next() | ((long)rng.Next() << 32));
                var remainder = int64Divisor.AbsFloorRem(testDividend, out var rounded);
                Assert.AreEqual(testDividend % divisor, remainder, $"Trying to test {testDividend} % {divisor}");
                Assert.AreEqual(testDividend / divisor * divisor, rounded, $"Trying to test {testDividend} / {divisor} * {divisor}");
            }
        }
    }
}
