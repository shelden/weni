using System;
using System.IO;
using System.Text;

namespace DataCapture.IO
{
    /// <summary>
    /// Often programs need Temporary Files.  The System.IO 
    /// methods to create temporary files, sure, but not
    /// in a very OO way.  They work on Strings, for example, 
    /// FileInfos.
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
        #region members
        private FileInfo file_;
        private static DirectoryInfo dir_;
        #endregion

        #region static initialzer
        static TempFile()
        {
            if (System.IO.Directory.Exists("c:\\temp"))
            {
                dir_ = new DirectoryInfo("c:\\temp");
            }
            else if (System.IO.Directory.Exists("/tmp"))
            {
                dir_ = new DirectoryInfo("/tmp");
            }
            else
            {
                dir_ = new DirectoryInfo(Path.GetTempPath());
            }
        }
        #endregion

        #region properties
        public FileInfo Value { get { return file_; } }
        public String FullName { get { return file_.FullName; } }
        #endregion

        #region constructors
        public TempFile(String suffix = null)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(dir_.FullName);
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
            file_ = new FileInfo(sb.ToString());
            this.Touch();
        }
        #endregion

        #region Disposable
        public void ReallyDelete()
        {
            if (file_ == null) return;
            if (!System.IO.File.Exists(file_.FullName)) return;
            try
            {
                System.IO.File.Delete(file_.FullName);
            }
            catch
            {
                /* no code; deliberately suppress error per contract */
            }
            file_ = null;
        }
        public void Dispose()
        {
            this.ReallyDelete();
        }

        #endregion

        #region behavior
        private void Touch()
        {
             using (var fs = File.Open(file_.FullName
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