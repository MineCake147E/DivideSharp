using System.Collections.Generic;

using NUnit.Framework;

namespace DivideSharp.Tests
{
    [TestFixture]
    public class UInt32DivisionTests
    {
        #region Test Cases

        private static IEnumerable<TestCaseData> DivisionTestCaseSource
        {
            get
            {
                var divisors = new uint[]
                {
                    //Obvious
                    1,
                    //Pre-Defined
                    3,4,5,6,7,8,9,10,11,12,
                    //Shift
                    2,16,
                    19, //MultiplyAdd
                    23  //Multiply
                };
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
                yield return new TestCaseData(0x8000_0001u, 1u);
                yield return new TestCaseData(0x8000_0001u, 0x8000_0000u);
                yield return new TestCaseData(0x8000_0001u, 0x8000_0001u);
                yield return new TestCaseData(0x8000_0001u, 0x8000_0002u);
                yield return new TestCaseData(0x8000_0001u, 0xFFFF_FFFEu);
                yield return new TestCaseData(0x8000_0001u, 0xFFFF_FFFFu);
            }
        }

        #endregion Test Cases

        [TestCaseSource(nameof(DivisionTestCaseSource))]
        public void CorrectlyDivides(uint divisor, uint testDividend)
        {
            var uInt32Divisor = new UInt32Divisor(divisor);
            var quotient = testDividend / uInt32Divisor;
            Assert.AreEqual(testDividend / divisor, quotient);
        }

        [TestCaseSource(nameof(DivisionTestCaseSource))]
        public void CalculatesModulusCorrectly(uint divisor, uint testDividend)
        {
            var uInt32Divisor = new UInt32Divisor(divisor);
            var remainder = testDividend % uInt32Divisor;
            Assert.AreEqual(testDividend % divisor, remainder);
        }

        [TestCaseSource(nameof(DivisionTestCaseSource))]
        public void DivRemReturnsCorrectly(uint divisor, uint testDividend)
        {
            var uInt32Divisor = new UInt32Divisor(divisor);
            var remainder = uInt32Divisor.DivRem(testDividend, out var quotient);
            Assert.Multiple(() =>
            {
                Assert.AreEqual(testDividend % divisor, remainder);
                Assert.AreEqual(testDividend / divisor, quotient);
            });
        }

        [TestCaseSource(nameof(DivisionTestCaseSource))]
        public void CalculatesFloorCorrectly(uint divisor, uint testDividend)
        {
            var uInt32Divisor = new UInt32Divisor(divisor);
            var rounded = uInt32Divisor.Floor(testDividend);
            Assert.AreEqual(testDividend / divisor * divisor, rounded);
        }

        [TestCaseSource(nameof(DivisionTestCaseSource))]
        public void CalculatesFloorRemCorrectly(uint divisor, uint testDividend)
        {
            var uInt32Divisor = new UInt32Divisor(divisor);
            var remainder = uInt32Divisor.FloorRem(testDividend, out var rounded);
            Assert.Multiple(() =>
            {
                Assert.AreEqual(testDividend % divisor, remainder);
                Assert.AreEqual(testDividend / divisor * divisor, rounded);
            });
        }
    }
}
