﻿using NUnit.Framework;
using System;

namespace LM.DataCapture.Common.ExampleTest
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
      Console.WriteLine("...or Test Explorer");
      Console.WriteLine("...simply uncomment the next line in ");
      Console.WriteLine(this.GetType().FullName);
      //Assert.AreEqual(1, -1);
    }
  }
}
