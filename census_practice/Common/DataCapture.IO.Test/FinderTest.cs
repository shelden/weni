using System;
using System.IO;
using System.Collections.Generic;
using DataCapture.IO;
using NUnit.Framework;

namespace DataCapture.IO.Test
{
  public class FinderTest
  {
    [Test()]
    public void CanCreate()
    {
      var finder0 = new FileFinder();
      Assert.IsNotNull(finder0);
      var finder1 = new FileFinder(new DirectoryInfo("\\"));
      Assert.IsNotNull(finder1);
      var finder2 = new FileFinder();
      Assert.IsNotNull(finder2);
    }

    [Test()]
    public void FindsThings()
    {
      var r = new Random();
      var files = new List<TempFile>();
      try
      {
        String extension = "" + r.Next();
        var finder = new FileFinder(TempFile.DIRECTORY);
        int before = finder.Search("*." + extension).Count;

        // create some files:
        // (small race condition if someone else is also creating
        // files with this extension.  But hopefully the random
        // extension make that unlikely)
        int toCreate = r.Next(2, 5);
        for (int i = 0; i < toCreate; i++)
        {
          files.Add(new TempFile(extension));
          Console.WriteLine(i
            + " of "
            + toCreate
            + ") "
            + files[i]
            );
        }

        int after = finder.Search("*." + extension).Count;
        Assert.That(before + toCreate == after
            , "before there were "
            + before
            + " files with the exension ["
            + extension
            + "], and now there are "
            + after
            + " when we created "
            + toCreate
            );
      }
      finally
      {
        foreach (var temp in files)
        {
          temp.ReallyDelete();
        }
      }
    }
  }
}


