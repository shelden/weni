
using System;
using System.IO;
using System.Text;

namespace DataCapture.Build.AssemblyVersionSetter
{
    /// <summary>
    /// XXX add code to remove the file on destruction / GC
    /// </summary>
    public class TempFile
    {
        #region members
        private FileInfo file_;
        private static DirectoryInfo dir_;
        #endregion

        #region static initialzer
        static TempFile()
        {
            if (System.IO.Directory.Exists("/tmp"))
            {
                dir_ = new DirectoryInfo("/tmp");
            }
            else if (System.IO.Directory.Exists("c:\\temp"))
            {
                dir_ = new DirectoryInfo("c:\\temp");
                return;
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
        }
        #endregion
    }
}