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
            String step1Name = TestUtil.NextString();
            String step0Name = TestUtil.NextString();
            String queueName = TestUtil.NextString();
            String mapName = TestUtil.NextString();
            int type = TestUtil.RANDOM.Next(2, 100);

            var queue = Queue.Insert(dbConn, queueName);
            var map = Map.Insert(dbConn, mapName);
            var step1 = Step.Insert(dbConn, step1Name, map, queue, type);
            var step0 = Step.Insert(dbConn, step0Name, map, queue, step1, type);
            var stepN = Step.Select(dbConn, step0Name);

            var rule = Rule.Insert(dbConn, "a", Rule.Compare.LESS, "132", 7, step0, step1);
            var ruleN = Rule.Select(dbConn, rule.Id);
            
            Console.WriteLine(step0 == null ? "[null]" : step0.ToString());
            Console.WriteLine(step1 == null ? "[null]" : step1.ToString());
            Console.WriteLine(stepN == null ? "[null]" : stepN.ToString());
            Console.WriteLine(rule.ToString());
            Console.WriteLine(ruleN.ToString());
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
