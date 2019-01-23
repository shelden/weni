using System;
using LM.DataCapture.Common.FileUtil;
using NUnit.Framework;

namespace LM.DataCapture.Common.FileUtil.Test
{
  public class TempFileTest
  {
    [Test()]
    public void CanCreate()
    {
      var tmp = new TempFile(".xyz");
      Assert.IsTrue(System.IO.File.Exists(tmp.FullName));
      tmp.ReallyDelete();
    }

    [Test()]
    public void GoneWhenDisposed()
    {
      String copy;
      using (var tmp = new TempFile("xyz"))
      {
        copy = tmp.Value.FullName;
        Assert.IsTrue(System.IO.File.Exists(tmp.FullName));
      }
      Assert.IsFalse(System.IO.File.Exists(copy));
    }

    [Test()]
    public void DotInExtensionIsOptional()
    {
      String suffix = "something";
      using (var noDot = new TempFile(suffix))
      {
        using (var withDot = new TempFile("." + suffix))
        {
          Assert.IsTrue(System.IO.File.Exists(noDot.FullName));
          Assert.IsTrue(System.IO.File.Exists(withDot.FullName));
          Assert.AreNotEqual(noDot.FullName, withDot.FullName);
          Assert.AreNotEqual(noDot.Value, withDot.Value);

          // and the important part of the test, make sure that
          // the file with a dot in the constructor has the same
          // value as the one that didn't specify
          Assert.AreEqual(noDot.Value.Extension, withDot.Value.Extension);
        }
      }
    }

    [Test()]
    public void MultiplesAreDifferent()
    {
      String extension = "gobucks";
      using (var tmp0 = new TempFile(extension))
      {
        using (var tmp1 = new TempFile(extension))
        {
          Assert.IsTrue(System.IO.File.Exists(tmp0.FullName));
          Assert.IsTrue(System.IO.File.Exists(tmp1.FullName));
          Assert.AreNotEqual(tmp0.Value, tmp1.Value);
          Assert.AreNotEqual(tmp0.FullName, tmp1.FullName);
        }
      }
    }

    [Test()]
    public void BaseDirectoryCorrectlyCalculated()
    {
      Assert.That(System.IO.Directory.Exists(TempFile.DIRECTORY.FullName));
    }
  }
}

