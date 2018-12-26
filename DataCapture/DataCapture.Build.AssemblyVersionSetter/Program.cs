using System;
using System.Text;

namespace DataCapture.Build.AssemblyVersionSetter
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
            Console.WriteLine("--> DataCapture.Build.AssemblyVersionSetter()");
            Console.WriteLine("<-- DataCapture.Build.AssemblyVersionSetter()");
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
