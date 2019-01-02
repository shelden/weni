using System;
using System.IO;
using DataCapture.IO;

namespace DataCapture.Build.VersionSetter
{
    public class Program
    {
        #region Constants
        #endregion

        #region Members
        private String company_ = "Vermont Purple, LLC";
        private String version_ = "0.10.*";
        private DirectoryInfo top_ = new DirectoryInfo(".");
        #endregion

        #region Constructor
        public Program(String[] argv)
        { 
            for(int i = 0; argv != null && i < argv.Length; i++)
            {
                if ("--version".Equals(argv[i]))
                {
                    version_ = argv[++i];
                }
                else if ("--company".Equals(argv[i]))
                {
                    company_ = argv[++i];
                }
                else if ("--dir".Equals(argv[i]))
                {
                    top_ = new DirectoryInfo(argv[++i]);
                }
                else
                {
                    throw new Exception("Usage: "
                        + this.GetType().FullName
                        + " [--version version]"
                        + " [--dir topDirectory]"
                        + " [--company company]"
                        );
                }
            }
        }
        #endregion

        #region Go
        public void Go()
        {
            var setter = new VersionSetter(this.version_, this.company_);
            Console.WriteLine(setter);
            var ff = new FileFinder(this.top_);
            foreach (var file in ff.Search("AssemblyInfo.cs"))
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
