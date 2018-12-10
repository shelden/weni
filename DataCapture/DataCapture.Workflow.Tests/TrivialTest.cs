using NUnit.Framework;
using System;

namespace VtPurple.Misc.Tests
{
    public class TrivialTest
    {
        [Test()]
        public void TestTwoSquaredIsFour()
        {
            Assert.AreEqual(2 * 2, 4);
        }
        [Test()]
        public void TestThatFails()
        {
            //Assert.AreEqual(1, -1);
        }
    }
}
