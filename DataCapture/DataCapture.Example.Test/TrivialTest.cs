﻿using NUnit.Framework;
using System;

namespace DataCapture.Example.Test
{
    public class TrivialTest
    {
        [Test()]
        public void TwoSquaredIsFour()
        {
            Assert.That(2 * 2 == 4);
        }
        [Test()]
        public void SomethingThatFailsWhenUncommented()
        {
            Console.WriteLine("to make something fail on Jenkins....");
            Console.WriteLine("...simply uncomment the next line in ");
            Console.WriteLine(this.GetType().FullName);
            //Assert.AreEqual(1, -1);
        }
    }
}
