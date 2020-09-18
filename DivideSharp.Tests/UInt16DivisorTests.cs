using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

using NUnit.Framework;

using TestAndBenchmarkUtils;

namespace DivideSharp.Tests
{
    [TestFixture]
    public class UInt16DivisionTests
    {
        #region Test Cases

        private static IEnumerable<ushort> Divisors => new ushort[]
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
            29 //Multiply
        };

        private static IEnumerable<TestCaseData> RandomDivisionTestCaseSource => Divisors.Append((ushort)0x8001u).Select(a => new TestCaseData(a));

        private const ulong RandomTestCount = ushort.MaxValue + 1;

        private static IEnumerable<TestCaseData> DivisionTestCaseSource
        {
            get
            {
                var divisors = Divisors;
                //Generate Test Data's
                foreach (var item in divisors)
                {
                    yield return new TestCaseData(item, (ushort)1u);
                    yield return new TestCaseData(item, (ushort)(item - 1));
                    yield return new TestCaseData(item, item);
                    yield return new TestCaseData(item, (ushort)(item + 1));
                    yield return new TestCaseData(item, (ushort)(2 * item - 1));
                    yield return new TestCaseData(item, (ushort)(2 * item));
                    yield return new TestCaseData(item, (ushort)(2 * item + 1));
                }
                //Branch
                yield return new TestCaseData((ushort)0x8001u, (ushort)1u);
                yield return new TestCaseData((ushort)0x8001u, (ushort)0x8000u);
                yield return new TestCaseData((ushort)0x8001u, (ushort)0x8001u);
                yield return new TestCaseData((ushort)0x8001u, (ushort)0x8002u);
                yield return new TestCaseData((ushort)0x8001u, (ushort)0xFFFEu);
                yield return new TestCaseData((ushort)0x8001u, (ushort)0xFFFFu);
            }
        }

        #endregion Test Cases

        [TestCaseSource(nameof(DivisionTestCaseSource))]
        public void CorrectlyDivides(ushort divisor, ushort testDividend)
        {
            var uInt16Divisor = new UInt16Divisor(divisor);
            var quotient = testDividend / uInt16Divisor;
            Assert.AreEqual(testDividend / divisor, quotient);
        }

        [TestCaseSource(nameof(DivisionTestCaseSource))]
        public void CalculatesModulusCorrectly(ushort divisor, ushort testDividend)
        {
            var uInt16Divisor = new UInt16Divisor(divisor);
            var remainder = testDividend % uInt16Divisor;
            Assert.AreEqual(testDividend % divisor, remainder);
        }

        [TestCaseSource(nameof(DivisionTestCaseSource))]
        public void DivRemReturnsCorrectly(ushort divisor, ushort testDividend)
        {
            var uInt16Divisor = new UInt16Divisor(divisor);
            var remainder = uInt16Divisor.DivRem(testDividend, out var quotient);
            Assert.Multiple(() =>
            {
                Assert.AreEqual(testDividend % divisor, remainder);
                Assert.AreEqual(testDividend / divisor, quotient);
            });
        }

        [TestCaseSource(nameof(DivisionTestCaseSource))]
        public void CalculatesFloorCorrectly(ushort divisor, ushort testDividend)
        {
            var uInt16Divisor = new UInt16Divisor(divisor);
            var rounded = uInt16Divisor.Floor(testDividend);
            Assert.AreEqual(testDividend / divisor * divisor, rounded);
        }

        [TestCaseSource(nameof(DivisionTestCaseSource))]
        public void CalculatesFloorRemCorrectly(ushort divisor, ushort testDividend)
        {
            var uInt16Divisor = new UInt16Divisor(divisor);
            var remainder = uInt16Divisor.FloorRem(testDividend, out var rounded);
            Assert.Multiple(() =>
            {
                Assert.AreEqual(testDividend % divisor, remainder);
                Assert.AreEqual(testDividend / divisor * divisor, rounded);
            });
        }

        [TestCaseSource(nameof(RandomDivisionTestCaseSource))]
        public void CorrectlyDividesAllNumerators(ushort divisor)
        {
            var uInt16Divisor = new UInt16Divisor(divisor);
            for (ulong i = 0; i < RandomTestCount; i++)
            {
                var testDividend = (ushort)i;
                var quotient = testDividend / uInt16Divisor;
                Assert.AreEqual(testDividend / divisor, quotient, $"Trying to test {testDividend} / {divisor}");
            }
        }

        [TestCaseSource(nameof(RandomDivisionTestCaseSource))]
        public void CalculatesModulusCorrectlyAllNumerators(ushort divisor)
        {
            var uInt16Divisor = new UInt16Divisor(divisor);
            for (ulong i = 0; i < RandomTestCount; i++)
            {
                var testDividend = (ushort)i;
                var remainder = testDividend % uInt16Divisor;
                Assert.AreEqual(testDividend % divisor, remainder, $"Trying to test {testDividend} % {divisor}");
            }
        }

        [TestCaseSource(nameof(RandomDivisionTestCaseSource))]
        public void DivRemReturnsCorrectlyAllNumerators(ushort divisor)
        {
            var uInt16Divisor = new UInt16Divisor(divisor);
            for (ulong i = 0; i < RandomTestCount; i++)
            {
                var testDividend = (ushort)i;
                var remainder = uInt16Divisor.DivRem(testDividend, out var quotient);
                Assert.AreEqual(testDividend % divisor, remainder, $"Trying to test {testDividend} % {divisor}");
                Assert.AreEqual(testDividend / divisor, quotient, $"Trying to test {testDividend} / {divisor}");
            }
        }

        [TestCaseSource(nameof(RandomDivisionTestCaseSource))]
        public void CalculatesFloorCorrectlyAllNumerators(ushort divisor)
        {
            var uInt16Divisor = new UInt16Divisor(divisor);
            for (ulong i = 0; i < RandomTestCount; i++)
            {
                var testDividend = (ushort)i;
                var rounded = uInt16Divisor.Floor(testDividend);
                Assert.AreEqual(testDividend / divisor * divisor, rounded, $"Trying to test {testDividend} / {divisor} * {divisor}");
            }
        }

        [TestCaseSource(nameof(RandomDivisionTestCaseSource))]
        public void CalculatesFloorRemCorrectlyAllNumerators(ushort divisor)
        {
            var uInt16Divisor = new UInt16Divisor(divisor);
            for (ulong i = 0; i < RandomTestCount; i++)
            {
                var testDividend = (ushort)i;
                var remainder = uInt16Divisor.FloorRem(testDividend, out var rounded);
                Assert.AreEqual(testDividend % divisor, remainder, $"Trying to test {testDividend} % {divisor}");
                Assert.AreEqual(testDividend / divisor * divisor, rounded, $"Trying to test {testDividend} / {divisor} * {divisor}");
            }
        }
    }
}
