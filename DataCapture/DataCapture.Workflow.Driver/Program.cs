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
            var user = TestUtil.MakeUser(ConnectionFactory.Create());
            for (int i = 0; i < user.LoginLimit + 1; i++)
            {
                var conn0 = new DataCapture.Workflow.Connection();
                conn0.Connect(user.Login);
                Console.WriteLine(i + ") " + conn0);
            }


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
