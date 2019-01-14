using System;
using DataCapture.Workflow.Yeti.Db;
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
            String itemName0 = "early" + TestUtil.NextString();
            String itemName1 = "late" + TestUtil.NextString();
            int priority = TestUtil.RANDOM.Next(1, 100);
            var wfConn = TestUtil.CreateConnected();
            var names = TestUtil.CreateBasicMap();
            var pairs0 = TestUtil.CreatePairs();
            var pairs1 = TestUtil.CreatePairs();

            // first put in two items with same priority, but
            // different attributes
            wfConn.CreateItem(names["map"]
                , itemName0
                , names["startStep"]
                , pairs0
                , priority
                );
            wfConn.CreateItem(names["map"]
                , itemName1
                , names["startStep"]
                , pairs1
                , priority
            );

            // the earlier item should be retrieved first
            var item0 = wfConn.GetItem(names["queue"]);

            // the later item should be retrieved next
            var item1 = wfConn.GetItem(names["queue"]);

            Console.WriteLine(item0);
            Console.WriteLine(item1);

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
