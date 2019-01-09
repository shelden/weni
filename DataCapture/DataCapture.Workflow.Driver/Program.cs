using System;
using DataCapture.Workflow.Db;
using DataCapture.Workflow.Test;
using DataCapture.Workflow;

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
            var user = TestUtil.MakeUser(dbConn);
            Console.WriteLine(user);

            var map = TestUtil.MakeMap(dbConn);
            Console.WriteLine(map);

            var queue = TestUtil.MakeQueue(dbConn);
            Console.WriteLine(queue);

            var step0 = TestUtil.MakeStep(dbConn);
            Console.WriteLine(step0);

            var wfConn = new Connection();
            wfConn.Connect(user.Login);

            Console.WriteLine(wfConn);

            wfConn.CreateItem(map.Name, "foo", step0.Name, null);

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
