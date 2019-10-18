using NUnit.Framework;

namespace DivideSharp.Tests
{
    [TestFixture]
    public class DivisionTests
    {
        #region Test Cases

        [TestCase(9u, 1u)]
        [TestCase(9u, 8u)]
        [TestCase(9u, 9u)]
        [TestCase(9u, 10u)]
        [TestCase(9u, 17u)]
        [TestCase(9u, 18u)]
        [TestCase(9u, 19u)]
        [TestCase(16u, 1u)]
        [TestCase(16u, 15u)]
        [TestCase(16u, 16u)]
        [TestCase(16u, 17u)]
        [TestCase(16u, 31u)]
        [TestCase(16u, 32u)]
        [TestCase(16u, 33u)]
        [TestCase(19u, 1u)]
        [TestCase(19u, 18u)]
        [TestCase(19u, 19u)]
        [TestCase(19u, 20u)]
        [TestCase(19u, 37u)]
        [TestCase(19u, 38u)]
        [TestCase(19u, 39u)]
        [TestCase(23u, 1u)]
        [TestCase(23u, 22u)]
        [TestCase(23u, 23u)]
        [TestCase(23u, 24u)]
        [TestCase(23u, 45u)]
        [TestCase(23u, 46u)]
        [TestCase(23u, 47u)]

        #endregion Test Cases

        public void UInt32DivisorCorrectlyDivides(uint divisor, uint testDividend)
        {
            var uInt32Divisor = new UInt32Divisor(divisor);
            var quotient = uInt32Divisor.Divide(testDividend);
            Assert.AreEqual(testDividend / divisor, quotient);
        }
    }
}
