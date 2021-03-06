﻿using System;
using System.Collections.Generic;
using System.Linq;

using Newtonsoft.Json;

using NUnit.Framework;

using TestAndBenchmarkUtils;

namespace DivideSharp.Tests
{
    [TestFixture]
    public class Int32DivisionTests
    {
        #region Test Cases

        private static IEnumerable<int> Divisors
        {
            get
            {
                var divisors = new int[]
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
                return divisors.Concat(divisors.Select(a => -a)).Distinct().OrderBy(a => Math.Abs(a));
            }
        }

        private static IEnumerable<TestCaseData> RandomDivisionTestCaseSource => Divisors.Append(int.MinValue).Select(a => new TestCaseData(a));

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
            Console.WriteLine($"quotient:{quotient}");
            Assert.AreEqual(testDividend / divisor, quotient, SerializeDivisor(int32Divisor));
        }

        [TestCaseSource(nameof(DivisionTestCaseSource))]
        public void CalculatesModulusCorrectly(int divisor, int testDividend)
        {
            var int32Divisor = new Int32Divisor(divisor);
            var remainder = testDividend % int32Divisor;
            Console.WriteLine($"remainder:{remainder}");
            Assert.AreEqual(testDividend % divisor, remainder, SerializeDivisor(int32Divisor));
        }

        [TestCaseSource(nameof(DivisionTestCaseSource))]
        public void DivRemReturnsCorrectly(int divisor, int testDividend)
        {
            var int32Divisor = new Int32Divisor(divisor);
            var remainder = int32Divisor.DivRem(testDividend, out var quotient);
            Console.WriteLine($"quotient:{quotient}, remainder:{remainder}");
            Assert.Multiple(() =>
            {
                Assert.AreEqual(testDividend % divisor, remainder, SerializeDivisor(int32Divisor));
                Assert.AreEqual(testDividend / divisor, quotient, SerializeDivisor(int32Divisor));
            });
        }

        [TestCaseSource(nameof(DivisionTestCaseSource))]
        public void CalculatesAbsFloorCorrectly(int divisor, int testDividend)
        {
            var int32Divisor = new Int32Divisor(divisor);
            var rounded = int32Divisor.AbsFloor(testDividend);
            Console.WriteLine($"rounded:{rounded}");
            Assert.AreEqual(testDividend / divisor * divisor, rounded, SerializeDivisor(int32Divisor));
        }

        [TestCaseSource(nameof(DivisionTestCaseSource))]
        public void CalculatesAbsFloorRemCorrectly(int divisor, int testDividend)
        {
            var int32Divisor = new Int32Divisor(divisor);
            var remainder = int32Divisor.AbsFloorRem(testDividend, out var rounded);
            Console.WriteLine($"rounded:{rounded}, remainder:{remainder}");
            Assert.Multiple(() =>
            {
                Assert.AreEqual(testDividend % divisor, remainder, SerializeDivisor(int32Divisor));
                Assert.AreEqual(testDividend / divisor * divisor, rounded, SerializeDivisor(int32Divisor));
            });
        }

        [TestCaseSource(nameof(RandomDivisionTestCaseSource))]
        public void CorrectlyDividesRandomNumerators(int divisor)
        {
            var int32Divisor = new Int32Divisor(divisor);
            var rng = new PcgRandom();
            for (ulong i = 0; i < RandomTestCount; i++)
            {
                var testDividend = unchecked((int)rng.Next());
                var quotient = testDividend / int32Divisor;
                Assert.AreEqual(testDividend / divisor, quotient, $"Trying to test {testDividend} / {divisor}");
            }
        }

        [TestCaseSource(nameof(RandomDivisionTestCaseSource))]
        public void CalculatesModulusCorrectlyRandomNumerators(int divisor)
        {
            var int32Divisor = new Int32Divisor(divisor);
            var rng = new PcgRandom();

            for (ulong i = 0; i < RandomTestCount; i++)
            {
                var testDividend = unchecked((int)rng.Next());
                var remainder = testDividend % int32Divisor;
                Assert.AreEqual(testDividend % divisor, remainder, $"Trying to test {testDividend} % {divisor}");
            }
        }

        [TestCaseSource(nameof(RandomDivisionTestCaseSource))]
        public void DivRemReturnsCorrectlyRandomNumerators(int divisor)
        {
            var int32Divisor = new Int32Divisor(divisor);
            var rng = new PcgRandom();
            for (ulong i = 0; i < RandomTestCount; i++)
            {
                var testDividend = unchecked((int)rng.Next());
                var remainder = int32Divisor.DivRem(testDividend, out var quotient);
                Assert.AreEqual(testDividend % divisor, remainder, $"Trying to test {testDividend} % {divisor}");
                Assert.AreEqual(testDividend / divisor, quotient, $"Trying to test {testDividend} / {divisor}");
            }
        }

        [TestCaseSource(nameof(RandomDivisionTestCaseSource))]
        public void CalculatesAbsFloorCorrectlyRandomNumerators(int divisor)
        {
            var int32Divisor = new Int32Divisor(divisor);
            var rng = new PcgRandom();
            for (ulong i = 0; i < RandomTestCount; i++)
            {
                var testDividend = unchecked((int)rng.Next());
                var rounded = int32Divisor.AbsFloor(testDividend);
                Assert.AreEqual(testDividend / divisor * divisor, rounded, $"Trying to test {testDividend} / {divisor} * {divisor}");
            }
        }

        [TestCaseSource(nameof(RandomDivisionTestCaseSource))]
        public void CalculatesAbsFloorRemCorrectlyRandomNumerators(int divisor)
        {
            var int32Divisor = new Int32Divisor(divisor);
            var rng = new PcgRandom();
            for (ulong i = 0; i < RandomTestCount; i++)
            {
                var testDividend = unchecked((int)rng.Next());
                var remainder = int32Divisor.AbsFloorRem(testDividend, out var rounded);
                Assert.AreEqual(testDividend % divisor, remainder, $"Trying to test {testDividend} % {divisor}");
                Assert.AreEqual(testDividend / divisor * divisor, rounded, $"Trying to test {testDividend} / {divisor} * {divisor}");
            }
        }
    }
}
