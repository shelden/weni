using System;
using DataCapture.IO;
using NUnit.Framework;

namespace DataCapture.IO.Test
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
            using (var tmp = new TempFile(".xyz"))
            {
                copy = tmp.Value.FullName;
                Assert.IsTrue(System.IO.File.Exists(tmp.FullName));
            }
            Assert.IsFalse(System.IO.File.Exists(copy));
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
    }
}

