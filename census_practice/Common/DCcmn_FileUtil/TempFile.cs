using System;
using System.IO;
using System.Text;

namespace LM.DataCapture.Common.FileUtil
{
  /// <summary>
  /// Often programs need Temporary Files.  And naive programmers
  /// will just create a file in the current directory, and 
  /// eventually the name will be duplicated, and/or it will
  /// be left around evetually filling the disk.
  /// 
  /// There are, of course, System.IO methods to do this correctly.
  /// However, they were written in .NET 1.0, and work on strings, 
  /// instead of the then-future, more-OO objects like FileInfos.
  /// 
  /// This class creates a Temporary File, as a FileInfo, 
  /// with a specified suffix.  The file will be removed
  /// upon disposal / garbage collection in a RAII way.
  /// 
  /// I.e., if you need to recreate
  /// a temporary .xml file?  Do this:
  /// using(var tmp = new TempFile(".xml")) {
  ///      // process tmp
  /// }
  /// 
  /// tmp will be deleted when leaving that using statement.
  /// </summary>
  public class TempFile : IDisposable
  {
    #region constants
    public static readonly DirectoryInfo DIRECTORY;
    #endregion

    #region members
    private FileInfo m_file;
    #endregion

    #region static initialzer
    /// <summary>
    /// Set up the directory in which we create all our temporary files.
    /// (This allows it to be portable to Mono.)
    /// </summary>
    static TempFile()
    {
      if (System.IO.Directory.Exists("c:\\temp"))
      {
        DIRECTORY = new DirectoryInfo("c:\\temp");
      }
      else if (System.IO.Directory.Exists("/tmp"))
      {
        DIRECTORY = new DirectoryInfo("/tmp");
      }
      else
      {
        DIRECTORY = new DirectoryInfo(Path.GetTempPath());
      }
    }
    #endregion

    #region properties
    public FileInfo Value { get { return m_file; } }
    public String FullName { get { return m_file.FullName; } }
    #endregion

    #region constructors
    /// <summary>
    /// Create a temporary file, with an optional extension
    /// </summary>
    /// <param name="suffix">Suffix.</param>
    public TempFile(String suffix = null)
    {
      StringBuilder sb = new StringBuilder();
      sb.Append(DIRECTORY.FullName);
      sb.Append(Path.DirectorySeparatorChar);
      sb.Append(Guid.NewGuid().ToString());
      if (!String.IsNullOrEmpty(suffix))
      {
        if (suffix[0] == '.')
        {
          suffix = suffix.Substring(1);
        }
        sb.Append('.');
        sb.Append(suffix);
      }
      m_file = new FileInfo(sb.ToString());
      this.Touch();
    }
    #endregion

    #region Disposable
    /// <summary>
    /// Deletes the underlying file without throwing an exception
    /// </summary>
    public void ReallyDelete()
    {
      if (m_file == null) return;
      if (!System.IO.File.Exists(m_file.FullName)) return;
      try
      {
        System.IO.File.Delete(m_file.FullName);
      }
      catch
      {
        /* deliberately suppress error per contract */
      }
      m_file = null;
    }

    /// <summary>
    /// Releases all resources, in this case the underlying file, used by the <see cref="T:DataCapture.IO.TempFile"/> object.
    /// </summary>
    public void Dispose()
    {
      this.ReallyDelete();
    }
    #endregion

    #region ToString()
    public override string ToString()
    {
      if (m_file == null) return "";
      return m_file.FullName;
    }
    #endregion

    #region private behavior
    /// <summary>
    /// Updates the underlying file; which has the side effect of creating it
    /// if it doesn't exist.
    /// </summary>
    private void Touch()
    {
      using (var fs = File.Open(m_file.FullName
         , FileMode.OpenOrCreate
         , FileAccess.Write
         , FileShare.ReadWrite
         )
         )
      {
        fs.Close();
      }
    }
    #endregion
  }
}