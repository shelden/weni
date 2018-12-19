﻿using System;
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
            var session = TestUtil.makeSession(dbConn);
            var step = TestUtil.makeStep(dbConn);
            var item = WorkItem.Insert(dbConn, step, "foo", 0, 0, session);

            for (int i = 0; i < 10; i++)
            {
                var data = WorkItemData.Insert(dbConn, item, "var" + i, TestUtil.NextString());
                Console.WriteLine(data);
            }

            Console.WriteLine(session);
            Console.WriteLine(item);

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
