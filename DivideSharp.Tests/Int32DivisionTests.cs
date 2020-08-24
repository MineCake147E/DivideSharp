using System;
using System.Collections.Generic;
using System.Linq;

using Newtonsoft.Json;

using NUnit.Framework;

namespace DivideSharp.Tests
{
    [TestFixture]
    public class Int32DivisionTests
    {
        #region Test Cases

        private static IEnumerable<TestCaseData> DivisionTestCaseSource
        {
            get
            {
                var divisorsNegatable = new int[]
                {
                    //Obvious
                    1,
                    //Pre-Defined
                    3,4,5,6,7,8,9,10,11,12,
                    //Shift
                    2,16,
                    15, //MultiplyAdd
                    21  //Multiply
                };
                //Generate Test Data's
                foreach (var item in divisorsNegatable.Concat(divisorsNegatable.Select(a => -a)).OrderBy(a => Math.Abs(a)))
                {
                    yield return new TestCaseData(item, 1);
                    yield return new TestCaseData(item, item - 1);
                    yield return new TestCaseData(item, item);
                    yield return new TestCaseData(item, item + 1);
                    yield return new TestCaseData(item, 2 * item - 1);
                    yield return new TestCaseData(item, 2 * item);
                    yield return new TestCaseData(item, 2 * item + 1);
                    yield return new TestCaseData(item, -item - 1);
                    yield return new TestCaseData(item, -item);
                    yield return new TestCaseData(item, -item + 1);
                    yield return new TestCaseData(item, 2 * -item - 1);
                    yield return new TestCaseData(item, 2 * -item);
                    yield return new TestCaseData(item, 2 * -item + 1);
                }
                //Branch
                yield return new TestCaseData(int.MinValue, 1);
                yield return new TestCaseData(int.MinValue, int.MinValue);
                yield return new TestCaseData(int.MinValue, -int.MaxValue);
                yield return new TestCaseData(int.MinValue, 0);
                yield return new TestCaseData(int.MinValue, int.MaxValue);
            }
        }

        #endregion Test Cases

        private static string SerializeDivisor(Int32Divisor divisor) => $"{JsonConvert.SerializeObject(divisor)}";

        [TestCaseSource(nameof(DivisionTestCaseSource))]
        public void CorrectlyDivides(int divisor, int testDividend)
        {
            var int32Divisor = new Int32Divisor(divisor);
            var quotient = testDividend / int32Divisor;
            Assert.AreEqual(testDividend / divisor, quotient, SerializeDivisor(int32Divisor));
        }

        [TestCaseSource(nameof(DivisionTestCaseSource))]
        public void CalculatesModulusCorrectly(int divisor, int testDividend)
        {
            var int32Divisor = new Int32Divisor(divisor);
            var remainder = testDividend % int32Divisor;
            Assert.AreEqual(testDividend % divisor, remainder, SerializeDivisor(int32Divisor));
        }

        [TestCaseSource(nameof(DivisionTestCaseSource))]
        public void DivRemReturnsCorrectly(int divisor, int testDividend)
        {
            var int32Divisor = new Int32Divisor(divisor);
            var remainder = int32Divisor.DivRem(testDividend, out var quotient);
            Assert.Multiple(() =>
            {
                Assert.AreEqual(testDividend % divisor, remainder, SerializeDivisor(int32Divisor));
                Assert.AreEqual(testDividend / divisor, quotient, SerializeDivisor(int32Divisor));
            });
        }

        [TestCaseSource(nameof(DivisionTestCaseSource))]
        public void CalculatesFloorCorrectly(int divisor, int testDividend)
        {
            var int32Divisor = new Int32Divisor(divisor);
            var rounded = int32Divisor.Floor(testDividend);
            Assert.AreEqual(testDividend / divisor * divisor, rounded, SerializeDivisor(int32Divisor));
        }

        [TestCaseSource(nameof(DivisionTestCaseSource))]
        public void CalculatesFloorRemCorrectly(int divisor, int testDividend)
        {
            var int32Divisor = new Int32Divisor(divisor);
            var remainder = int32Divisor.FloorRem(testDividend, out var rounded);
            Assert.Multiple(() =>
            {
                Assert.AreEqual(testDividend % divisor, remainder, SerializeDivisor(int32Divisor));
                Assert.AreEqual(testDividend / divisor * divisor, rounded, SerializeDivisor(int32Divisor));
            });
        }
    }
}
