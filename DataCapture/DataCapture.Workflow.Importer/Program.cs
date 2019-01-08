using System;
using System.Text;
using System.IO;
using System.Collections.Generic;
using DataCapture.Workflow.Db;

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
            files_.Add(new FileInfo("/tmp/foo.xml"));
        }
        #endregion

        #region Go
        public void Go()
        {
            Console.WriteLine("--> DataCapture.Workflow.Importer()");
            var dbConn = ConnectionFactory.Create();
            XmlImporter importer = new XmlImporter();
            foreach(var f in files_)
            {
                importer.Import(dbConn, f);
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
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
            }
            Console.WriteLine("Thank you for playing with "
                + (p == null ? "this program" : p.GetType().FullName)
                );
        }
        #endregion
    }
}
