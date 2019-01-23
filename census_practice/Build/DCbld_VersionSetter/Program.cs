using System;
using System.IO;
using LM.DataCapture.Common.FileUtil;

namespace LM.DataCapture.Build.VersionSetter
{
  public class Program
  {
    #region Constants
    #endregion

    #region Members
    private String m_company = "Vermont Purple, LLC";
    private String m_version = "0.0.*";
    private DirectoryInfo m_top = new DirectoryInfo(".");
    #endregion

    #region Constructor
    public Program(String[] argv)
    {
      for (int i = 0; argv != null && i < argv.Length; i++)
      {
        if ("--version".Equals(argv[i]))
        {
          m_version = argv[++i];
        }
        else if ("--company".Equals(argv[i]))
        {
          m_company = argv[++i];
        }
        else if ("--dir".Equals(argv[i])
                || "--top".Equals(argv[i])
            )
        {
          m_top = new DirectoryInfo(argv[++i]);
        }
        else
        {
          throw new Exception("Usage: "
              + this.GetType().FullName
              + " [--version version]"
              + " [--dir|--top topDirectory]"
              + " [--company company]"
              );
        }
      }
    }

    #endregion

    #region Go
    public void Go()
    {
      var setter = new VersionSetter(this.m_version, this.m_company);
      Console.WriteLine(setter);
      var ff = new FileFinder(this.m_top);
      foreach (var file in ff.Search("AssemblyInfo.cs"))
      {
        setter.EditInPlace(file);
      }
      foreach (var file in ff.Search("DC???_AssemblyInfo.cs"))
      {
        setter.EditInPlace(file);
      }
    }
    #endregion

    #region Main
    static void Main(string[] argv)
    {
      Program p = null;
      try
      {
        p = new Program(argv);
        p.Go();
      }
      catch (Exception ex)
      {
        Console.WriteLine(ex);
        Console.WriteLine(ex.StackTrace);
      }
      Console.WriteLine("Thank you for playing with "
          + (p == null ? "this program" : p.GetType().FullName)
          );
    }
    #endregion
  }
}

