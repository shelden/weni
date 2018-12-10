using System;
using System.Text;

namespace DataCapture.Workflow.Test
{
    public static class TestUtil
    {
        public static readonly Random RANDOM = new Random();
        public static String NextString(int length = 10)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < length; i++)
            {
                char letter = 'A';
                if (RANDOM.Next(0, 2) == 0)
                {
                    letter = 'a';
                }
                int offset = RANDOM.Next(0, 26);
                sb.Append((char)(letter + offset));
            }
            return sb.ToString();
        }

    }
}
