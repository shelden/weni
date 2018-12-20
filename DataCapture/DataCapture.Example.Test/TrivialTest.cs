using NUnit.Framework;
using System;

namespace DataCapture.Example.Test
{
    public class TrivialTest
    {
        [Test()]
        public void TwoSquaredIsFour()
        {
            Assert.AreEqual(2 * 2, 4);
        }
        [Test()]
        public void SomethingThatFails()
        {
            Assert.AreEqual(1, -1);
        }
    }
}
