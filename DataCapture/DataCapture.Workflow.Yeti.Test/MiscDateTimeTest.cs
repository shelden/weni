using System;
using System.Text;
using NUnit.Framework;
using DataCapture.Workflow.Yeti.Db;

namespace DataCapture.Workflow.Yeti.Test
{
    public class MiscDateTimeTest
    {
        [Test()]
        public void FlooredTimesAreSequential()
        {
            String format = DbUtil.FORMAT + "fff";
            // let's check 100 times just to be sure
            for (int i = 0; i < 100; i++)
            {
                var truncated = TestUtil.FlooredNow();
                var precise = DateTime.UtcNow;

                var msg = new StringBuilder();
                msg.Append(i);
                msg.Append(") ");
                msg.Append(truncated.ToString(format));
                msg.Append(" vs ");
                msg.Append(precise.ToString(format));

                Console.WriteLine(msg);

                // precise is taken after truncated, so you'd
                // think precise > truncated, right?

                Assert.GreaterOrEqual(precise, truncated, msg.ToString());
                Assert.GreaterOrEqual(precise.Millisecond, truncated.Millisecond, msg.ToString());

                int deltaMs = precise.Millisecond - truncated.Millisecond;
                Assert.LessOrEqual(deltaMs, 2, msg.ToString());
                Assert.GreaterOrEqual(deltaMs, 0, msg.ToString());
            }
        }
    }
}
