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
    }
}

