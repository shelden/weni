using System;
using System.Text;
using System.IO;
using System.Collections.Generic;

namespace DataCapture.Workflow.Importer
{
    public class Program
    {
        #region Constants
        #endregion

        #region Members
        IList<FileInfo> files_ = new List<FileInfo>();
        #endregion

        #region Constructor
        public Program(String[] argv)
        {
            foreach (var s in argv)
            {
                var f = new FileInfo(s);
                files_.Add(f);
            }
        }
        #endregion

        #region Go
        public void Go()
        {
            Console.WriteLine("--> DataCapture.Workflow.Importer()");
            foreach(var f in files_)
            {
                Console.WriteLine(f.FullName);
            }
            Console.WriteLine("<-- DataCapture.Workflow.Importer()");
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
