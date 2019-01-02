using System;
using System.Text;

namespace DataCapture.IO
{
    public class Example
    {
        #region Members
        #endregion

        #region Constructors
        #endregion

        #region ToString
        public override String ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(this.GetType().FullName);
            return sb.ToString();
        }
        #endregion
    }
}
