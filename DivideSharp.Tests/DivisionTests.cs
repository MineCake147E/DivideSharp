using NUnit.Framework;

namespace DivideSharp.Tests
{
    [TestFixture]
    public class DivisionTests
    {
        #region Test Cases

        //Pre-Defined
        [TestCase(9u, 1u)]
        [TestCase(9u, 8u)]
        [TestCase(9u, 9u)]
        [TestCase(9u, 10u)]
        [TestCase(9u, 17u)]
        [TestCase(9u, 18u)]
        [TestCase(9u, 19u)]
        //Shift
        [TestCase(16u, 1u)]
        [TestCase(16u, 15u)]
        [TestCase(16u, 16u)]
        [TestCase(16u, 17u)]
        [TestCase(16u, 31u)]
        [TestCase(16u, 32u)]
        [TestCase(16u, 33u)]
        //MultiplyAdd
        [TestCase(7u, 1u)]
        [TestCase(7u, 18u)]
        [TestCase(7u, 19u)]
        [TestCase(7u, 20u)]
        [TestCase(7u, 37u)]
        [TestCase(7u, 38u)]
        [TestCase(7u, 39u)]
        [TestCase(19u, 1u)]
        [TestCase(19u, 18u)]
        [TestCase(19u, 19u)]
        [TestCase(19u, 20u)]
        [TestCase(19u, 37u)]
        [TestCase(19u, 38u)]
        [TestCase(19u, 39u)]
        //Multiply
        [TestCase(23u, 1u)]
        [TestCase(23u, 22u)]
        [TestCase(23u, 23u)]
        [TestCase(23u, 24u)]
        [TestCase(23u, 45u)]
        [TestCase(23u, 46u)]
        [TestCase(23u, 47u)]
        //Branch
        [TestCase(0x8000_0001u, 1u)]
        [TestCase(0x8000_0001u, 0x8000_0000u)]
        [TestCase(0x8000_0001u, 0x8000_0001u)]
        [TestCase(0x8000_0001u, 0x8000_0002u)]
        [TestCase(0x8000_0001u, 0xFFFF_FFFEu)]
        [TestCase(0x8000_0001u, 0xFFFF_FFFFu)]

        #endregion Test Cases

        public void UInt32DivisorCorrectlyDivides(uint divisor, uint testDividend)
        {
            var uInt32Divisor = new UInt32Divisor(divisor);
            var quotient = testDividend / uInt32Divisor;
            Assert.AreEqual(testDividend / divisor, quotient);
        }

        #region Test Cases

        //Pre-Defined
        [TestCase(9u, 1u)]
        [TestCase(9u, 8u)]
        [TestCase(9u, 9u)]
        [TestCase(9u, 10u)]
        [TestCase(9u, 17u)]
        [TestCase(9u, 18u)]
        [TestCase(9u, 19u)]
        //Shift
        [TestCase(16u, 1u)]
        [TestCase(16u, 15u)]
        [TestCase(16u, 16u)]
        [TestCase(16u, 17u)]
        [TestCase(16u, 31u)]
        [TestCase(16u, 32u)]
        [TestCase(16u, 33u)]
        //MultiplyAdd
        [TestCase(19u, 1u)]
        [TestCase(19u, 18u)]
        [TestCase(19u, 19u)]
        [TestCase(19u, 20u)]
        [TestCase(19u, 37u)]
        [TestCase(19u, 38u)]
        [TestCase(19u, 39u)]
        //Multiply
        [TestCase(23u, 1u)]
        [TestCase(23u, 22u)]
        [TestCase(23u, 23u)]
        [TestCase(23u, 24u)]
        [TestCase(23u, 45u)]
        [TestCase(23u, 46u)]
        [TestCase(23u, 47u)]
        //Branch
        [TestCase(0x8000_0001u, 1u)]
        [TestCase(0x8000_0001u, 0x8000_0000u)]
        [TestCase(0x8000_0001u, 0x8000_0001u)]
        [TestCase(0x8000_0001u, 0x8000_0002u)]
        [TestCase(0x8000_0001u, 0xFFFF_FFFEu)]
        [TestCase(0x8000_0001u, 0xFFFF_FFFFu)]

        #endregion Test Cases

        public void UInt32DivisorCalculatesModulusCorrectly(uint divisor, uint testDividend)
        {
            var uInt32Divisor = new UInt32Divisor(divisor);
            var remainder = testDividend % uInt32Divisor;
            Assert.AreEqual(testDividend % divisor, remainder);
        }

        #region Test Cases

        //Pre-Defined
        [TestCase(9u, 1u)]
        [TestCase(9u, 8u)]
        [TestCase(9u, 9u)]
        [TestCase(9u, 10u)]
        [TestCase(9u, 17u)]
        [TestCase(9u, 18u)]
        [TestCase(9u, 19u)]
        //Shift
        [TestCase(16u, 1u)]
        [TestCase(16u, 15u)]
        [TestCase(16u, 16u)]
        [TestCase(16u, 17u)]
        [TestCase(16u, 31u)]
        [TestCase(16u, 32u)]
        [TestCase(16u, 33u)]
        //MultiplyAdd
        [TestCase(19u, 1u)]
        [TestCase(19u, 18u)]
        [TestCase(19u, 19u)]
        [TestCase(19u, 20u)]
        [TestCase(19u, 37u)]
        [TestCase(19u, 38u)]
        [TestCase(19u, 39u)]
        //Multiply
        [TestCase(23u, 1u)]
        [TestCase(23u, 22u)]
        [TestCase(23u, 23u)]
        [TestCase(23u, 24u)]
        [TestCase(23u, 45u)]
        [TestCase(23u, 46u)]
        [TestCase(23u, 47u)]
        //Branch
        [TestCase(0x8000_0001u, 1u)]
        [TestCase(0x8000_0001u, 0x8000_0000u)]
        [TestCase(0x8000_0001u, 0x8000_0001u)]
        [TestCase(0x8000_0001u, 0x8000_0002u)]
        [TestCase(0x8000_0001u, 0xFFFF_FFFEu)]
        [TestCase(0x8000_0001u, 0xFFFF_FFFFu)]

        #endregion Test Cases

        public void UInt32DivisorDivRemReturnsCorrectly(uint divisor, uint testDividend)
        {
            var uInt32Divisor = new UInt32Divisor(divisor);
            var remainder = uInt32Divisor.DivRem(testDividend, out var quotient);
            Assert.Multiple(() =>
            {
                Assert.AreEqual(testDividend % divisor, remainder);
                Assert.AreEqual(testDividend / divisor, quotient);
            });
        }

        #region Test Cases

        //Pre-Defined
        [TestCase(9u, 1u)]
        [TestCase(9u, 8u)]
        [TestCase(9u, 9u)]
        [TestCase(9u, 10u)]
        [TestCase(9u, 17u)]
        [TestCase(9u, 18u)]
        [TestCase(9u, 19u)]
        //Shift
        [TestCase(16u, 1u)]
        [TestCase(16u, 15u)]
        [TestCase(16u, 16u)]
        [TestCase(16u, 17u)]
        [TestCase(16u, 31u)]
        [TestCase(16u, 32u)]
        [TestCase(16u, 33u)]
        //MultiplyAdd
        [TestCase(19u, 1u)]
        [TestCase(19u, 18u)]
        [TestCase(19u, 19u)]
        [TestCase(19u, 20u)]
        [TestCase(19u, 37u)]
        [TestCase(19u, 38u)]
        [TestCase(19u, 39u)]
        //Multiply
        [TestCase(23u, 1u)]
        [TestCase(23u, 22u)]
        [TestCase(23u, 23u)]
        [TestCase(23u, 24u)]
        [TestCase(23u, 45u)]
        [TestCase(23u, 46u)]
        [TestCase(23u, 47u)]
        //Branch
        [TestCase(0x8000_0001u, 1u)]
        [TestCase(0x8000_0001u, 0x8000_0000u)]
        [TestCase(0x8000_0001u, 0x8000_0001u)]
        [TestCase(0x8000_0001u, 0x8000_0002u)]
        [TestCase(0x8000_0001u, 0xFFFF_FFFEu)]
        [TestCase(0x8000_0001u, 0xFFFF_FFFFu)]

        #endregion Test Cases

        public void UInt32DivisorCalculatesFloorCorrectly(uint divisor, uint testDividend)
        {
            var uInt32Divisor = new UInt32Divisor(divisor);
            var rounded = uInt32Divisor.Floor(testDividend);
            Assert.AreEqual(testDividend / divisor * divisor, rounded);
        }

        #region Test Cases

        //Pre-Defined
        [TestCase(9u, 1u)]
        [TestCase(9u, 8u)]
        [TestCase(9u, 9u)]
        [TestCase(9u, 10u)]
        [TestCase(9u, 17u)]
        [TestCase(9u, 18u)]
        [TestCase(9u, 19u)]
        //Shift
        [TestCase(16u, 1u)]
        [TestCase(16u, 15u)]
        [TestCase(16u, 16u)]
        [TestCase(16u, 17u)]
        [TestCase(16u, 31u)]
        [TestCase(16u, 32u)]
        [TestCase(16u, 33u)]
        //MultiplyAdd
        [TestCase(19u, 1u)]
        [TestCase(19u, 18u)]
        [TestCase(19u, 19u)]
        [TestCase(19u, 20u)]
        [TestCase(19u, 37u)]
        [TestCase(19u, 38u)]
        [TestCase(19u, 39u)]
        //Multiply
        [TestCase(23u, 1u)]
        [TestCase(23u, 22u)]
        [TestCase(23u, 23u)]
        [TestCase(23u, 24u)]
        [TestCase(23u, 45u)]
        [TestCase(23u, 46u)]
        [TestCase(23u, 47u)]
        //Branch
        [TestCase(0x8000_0001u, 1u)]
        [TestCase(0x8000_0001u, 0x8000_0000u)]
        [TestCase(0x8000_0001u, 0x8000_0001u)]
        [TestCase(0x8000_0001u, 0x8000_0002u)]
        [TestCase(0x8000_0001u, 0xFFFF_FFFEu)]
        [TestCase(0x8000_0001u, 0xFFFF_FFFFu)]

        #endregion Test Cases

        public void UInt32DivisorCalculatesFloorRemCorrectly(uint divisor, uint testDividend)
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
