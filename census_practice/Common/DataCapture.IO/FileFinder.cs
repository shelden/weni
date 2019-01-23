using System;
using System.IO;
using System.Collections.Generic;

namespace DataCapture.IO
{
  /// <summary>
  /// This class will recursively find files -- as FileInfos -- starting
  /// at an specified Directory
  /// </summary>
  public class FileFinder
  {
    #region members
    private DirectoryInfo m_top;
    #endregion

    #region constructors
    /// <summary>
    /// Create an object that can search for files, recursively, starting
    /// a top directory.
    /// </summary>
    /// <param name="top">The top directory</param>
    public FileFinder(DirectoryInfo top)
    {
      m_top = top;
    }
    /// <summary>
    /// Create an object that can search for files, recursively, starting
    /// a top directory.
    /// </summary>
    /// <param name="topDirectoryName">The top directory</param>
    public FileFinder(String topDirectoryName)
        : this(new DirectoryInfo(topDirectoryName))
    { /* no code */ }
    /// <summary>
    /// Create an object that can search for files, recursively, starting
    /// at the current working directory
    /// </summary>
    public FileFinder()
      : this(new DirectoryInfo("."))
    { /* no code */ }

    #endregion

    #region public behavior
    public IList<FileInfo> Search(String pattern)
    {
      var tmp = new List<FileInfo>();
      Search(tmp, m_top, pattern);
      return tmp;
    }
    #endregion

    #region private behavior
    /// <summary>
    /// Recursively search for files matching a pattern, starting at the
    /// specified directory
    /// </summary>
    /// <param name="dest">Destination.</param>
    /// <param name="dir">Dir.</param>
    /// <param name="pattern">Pattern.</param>
    private void Search(IList<FileInfo> dest, DirectoryInfo dir, String pattern)
    {
      foreach (string s in Directory.EnumerateFiles(dir.FullName, pattern))
      {
        dest.Add(new FileInfo(s));
      }
      foreach (string s in Directory.EnumerateDirectories(dir.FullName))
      {
        Search(dest, new DirectoryInfo(s), pattern);
      }
    }
    #endregion
  }
}

