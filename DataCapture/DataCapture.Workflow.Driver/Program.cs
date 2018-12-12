using System;
using DataCapture.Workflow.Db;

namespace DataCapture.Workflow.Driver
{
    public class Program
    {
        #region Constants
        public static readonly String SCMID_ = "$Id$";
        #endregion

        #region Members
        String[] argv_;
        #endregion

        #region Constructor
        public Program(String[] argv)
        {
            argv_ = argv;
        }
        #endregion

        #region Go
        public void Go()
        {
            Console.WriteLine("--> DataCapture.Workflow.Driver()");
            var dbConn = ConnectionFactory.Create();
            var queue = Queue.Select(dbConn, "stuff");
            if (queue == null)
            {
                Queue.Insert(dbConn, "stuff");
                queue = Queue.Select(dbConn, "stuff");
            }


            Console.WriteLine(queue == null ? "[null]" : queue.ToString());
            Console.WriteLine("<-- DataCapture.Workflow.Driver()");
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
