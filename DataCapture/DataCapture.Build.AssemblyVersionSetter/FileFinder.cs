using System;
using System.IO;
using System.Collections.Generic;

namespace DataCapture.Build.AssemblyVersionSetter
{
    public class FileFinder
    {
        #region members
        private DirectoryInfo dir_;
        #endregion

        #region constructors
        public FileFinder(DirectoryInfo dir)
        {
            dir_ = dir;
        }
        public FileFinder(String dirname)
            : this(new DirectoryInfo(dirname))
        { /* no code */ }

        #endregion

        #region behavior
        public IList<FileInfo> Search(String pattern)
        {
            var tmp = new List<FileInfo>();
            Search(tmp, dir_, pattern);
            return tmp;
        }

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
