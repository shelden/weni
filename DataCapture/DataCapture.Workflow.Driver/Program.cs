using System;
using DataCapture.Workflow.Db;
using DataCapture.Workflow.Test;

namespace DataCapture.Workflow.Driver
{
    public class Program
    {
        #region Constants
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
            var name = TestUtil.NextString();
            var queue0 = Queue.Insert(dbConn, TestUtil.NextString(), true);
            var queue1 = Queue.Insert(dbConn, TestUtil.NextString(), false);
            var queue2 = Queue.Insert(dbConn, TestUtil.NextString());
            var queueN = Queue.Select(dbConn, queue1.Name);

            Console.WriteLine(queue0);
            Console.WriteLine(queue1);
            Console.WriteLine(queue2);
            Console.WriteLine(queueN);

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
                while (ex != null)
                {
                    Console.WriteLine(ex.Message);
                    Console.WriteLine(ex.StackTrace);
                    ex = ex.InnerException;
                }
            }
            Console.WriteLine("Thank you for playing with "
                + (p == null ? "this program" : p.GetType().FullName)
                );
        }
        #endregion
    }
}
