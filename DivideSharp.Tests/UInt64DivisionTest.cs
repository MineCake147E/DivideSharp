using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

using Newtonsoft.Json;

using NUnit.Framework;

using TestAndBenchmarkUtils;

namespace DivideSharp.Tests
{
    [TestFixture]
    public class UInt64DivisionTests
    {
        #region Test Cases

        private static IEnumerable<ulong> Divisors => new ulong[]
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
            19, //MultiplyAdd
            23 //Multiply
        };

        private static IEnumerable<TestCaseData> RandomDivisionTestCaseSource => Divisors.Append(0x8000_0000_0000_0001u).Select(a => new TestCaseData(a));

        private const ulong RandomTestCount = 0x1_0000ul;

        private static IEnumerable<TestCaseData> DivisionTestCaseSource
        {
            get
            {
                var divisors = Divisors;
                //Generate Test Data's
                foreach (var item in divisors)
                {
                    yield return new TestCaseData(item, 1u);
                    yield return new TestCaseData(item, item - 1);
                    yield return new TestCaseData(item, item);
                    yield return new TestCaseData(item, item + 1);
                    yield return new TestCaseData(item, 2 * item - 1);
                    yield return new TestCaseData(item, 2 * item);
                    yield return new TestCaseData(item, 2 * item + 1);
                }
                //Branch
                yield return new TestCaseData(0x8000_0000_0000_0001u, 1u);
                yield return new TestCaseData(0x8000_0000_0000_0001u, 0x8000_0000_0000_0000u);
                yield return new TestCaseData(0x8000_0000_0000_0001u, 0x8000_0000_0000_0001u);
                yield return new TestCaseData(0x8000_0000_0000_0001u, 0x8000_0000_0000_0002u);
                yield return new TestCaseData(0x8000_0000_0000_0001u, 0xFFFF_FFFF_FFFF_FFFEu);
                yield return new TestCaseData(0x8000_0000_0000_0001u, 0xFFFF_FFFF_FFFF_FFFFu);
            }
        }

        #endregion Test Cases

        private static string SerializeDivisor(UInt64Divisor divisor) => $"{JsonConvert.SerializeObject(divisor)}";

        [TestCaseSource(nameof(DivisionTestCaseSource))]
        public void CorrectlyDivides(ulong divisor, ulong testDividend)
        {
            var uInt64Divisor = new UInt64Divisor(divisor);
            var quotient = testDividend / uInt64Divisor;
            Assert.AreEqual(testDividend / divisor, quotient, SerializeDivisor(uInt64Divisor));
        }

        [TestCaseSource(nameof(DivisionTestCaseSource))]
        public void CalculatesModulusCorrectly(ulong divisor, ulong testDividend)
        {
            var uInt64Divisor = new UInt64Divisor(divisor);
            var remainder = testDividend % uInt64Divisor;
            Assert.AreEqual(testDividend % divisor, remainder, SerializeDivisor(uInt64Divisor));
        }

        [TestCaseSource(nameof(DivisionTestCaseSource))]
        public void DivRemReturnsCorrectly(ulong divisor, ulong testDividend)
        {
            var uInt64Divisor = new UInt64Divisor(divisor);
            var remainder = uInt64Divisor.DivRem(testDividend, out var quotient);
            Assert.Multiple(() =>
            {
                Assert.AreEqual(testDividend % divisor, remainder, SerializeDivisor(uInt64Divisor));
                Assert.AreEqual(testDividend / divisor, quotient, SerializeDivisor(uInt64Divisor));
            });
        }

        [TestCaseSource(nameof(DivisionTestCaseSource))]
        public void CalculatesFloorCorrectly(ulong divisor, ulong testDividend)
        {
            var uInt64Divisor = new UInt64Divisor(divisor);
            var rounded = uInt64Divisor.Floor(testDividend);
            Assert.AreEqual(testDividend / divisor * divisor, rounded, SerializeDivisor(uInt64Divisor));
        }

        [TestCaseSource(nameof(DivisionTestCaseSource))]
        public void CalculatesFloorRemCorrectly(ulong divisor, ulong testDividend)
        {
            var uInt64Divisor = new UInt64Divisor(divisor);
            var remainder = uInt64Divisor.FloorRem(testDividend, out var rounded);
            Assert.Multiple(() =>
            {
                Assert.AreEqual(testDividend % divisor, remainder, SerializeDivisor(uInt64Divisor));
                Assert.AreEqual(testDividend / divisor * divisor, rounded, SerializeDivisor(uInt64Divisor));
            });
        }

        [TestCaseSource(nameof(RandomDivisionTestCaseSource))]
        public void CorrectlyDividesRandomNumerators(ulong divisor)
        {
            var uInt64Divisor = new UInt64Divisor(divisor);
            var rng = new PcgRandom();
            for (ulong i = 0; i < RandomTestCount; i++)
            {
                var testDividend = rng.Next() | ((ulong)rng.Next() << 32);
                var quotient = testDividend / uInt64Divisor;
                Assert.AreEqual(testDividend / divisor, quotient, $"Trying to test {testDividend} / {SerializeDivisor(uInt64Divisor)}");
            }
        }

        [TestCaseSource(nameof(RandomDivisionTestCaseSource))]
        public void CalculatesModulusCorrectlyRandomNumerators(ulong divisor)
        {
            var uInt64Divisor = new UInt64Divisor(divisor);
            var rng = new PcgRandom();

            for (ulong i = 0; i < RandomTestCount; i++)
            {
                var testDividend = rng.Next() | ((ulong)rng.Next() << 32);
                var remainder = testDividend % uInt64Divisor;
                Assert.AreEqual(testDividend % divisor, remainder, $"Trying to test {testDividend} % {SerializeDivisor(uInt64Divisor)}");
            }
        }

        [TestCaseSource(nameof(RandomDivisionTestCaseSource))]
        public void DivRemReturnsCorrectlyRandomNumerators(ulong divisor)
        {
            var uInt64Divisor = new UInt64Divisor(divisor);
            var rng = new PcgRandom();
            for (ulong i = 0; i < RandomTestCount; i++)
            {
                var testDividend = rng.Next() | ((ulong)rng.Next() << 32);
                var remainder = uInt64Divisor.DivRem(testDividend, out var quotient);
                Assert.AreEqual(testDividend % divisor, remainder, $"Trying to test {testDividend} % {SerializeDivisor(uInt64Divisor)}");
                Assert.AreEqual(testDividend / divisor, quotient, $"Trying to test {testDividend} / {SerializeDivisor(uInt64Divisor)}");
            }
        }

        [TestCaseSource(nameof(RandomDivisionTestCaseSource))]
        public void CalculatesFloorCorrectlyRandomNumerators(ulong divisor)
        {
            var uInt64Divisor = new UInt64Divisor(divisor);
            var rng = new PcgRandom();
            for (ulong i = 0; i < RandomTestCount; i++)
            {
                var testDividend = rng.Next() | ((ulong)rng.Next() << 32);
                var rounded = uInt64Divisor.Floor(testDividend);
                Assert.AreEqual(testDividend / divisor * divisor, rounded, $"Trying to test {testDividend} / {SerializeDivisor(uInt64Divisor)} * {SerializeDivisor(uInt64Divisor)}");
            }
        }

        [TestCaseSource(nameof(RandomDivisionTestCaseSource))]
        public void CalculatesFloorRemCorrectlyRandomNumerators(ulong divisor)
        {
            var uInt64Divisor = new UInt64Divisor(divisor);
            var rng = new PcgRandom();
            for (ulong i = 0; i < RandomTestCount; i++)
            {
                var testDividend = rng.Next() | ((ulong)rng.Next() << 32);
                var remainder = uInt64Divisor.FloorRem(testDividend, out var rounded);
                Assert.AreEqual(testDividend % divisor, remainder, $"Trying to test {testDividend} % {SerializeDivisor(uInt64Divisor)}");
                Assert.AreEqual(testDividend / divisor * divisor, rounded, $"Trying to test {testDividend} / {SerializeDivisor(uInt64Divisor)} * {SerializeDivisor(uInt64Divisor)}");
            }
        }
    }
}
