// /*---------------------------------------------------------------------------------------*/
// If you viewing this code.....
// The current code is under construction.
// The reason you see this text is that lot of refactors/improvements have been identified and they will be implemented over the next iterations versions. 
// This is not a final product yet.
// /*---------------------------------------------------------------------------------------*/
using System;
using System.Text;

namespace ermeX.Tests.Common.RandomValues
{
    public static class RandomHelper
    {
        public static string GetRandomString(int maxLenght = 5)
        {
            var builder = new StringBuilder();
            var random = new Random();
            char ch;
            int size = GetRandomInt(1, maxLenght);
            for (int i = 0; i < size; i++)
            {
                ch = Convert.ToChar(Convert.ToInt32(Math.Floor(26 * random.NextDouble() + 65)));
                builder.Append(ch);
            }
            return builder.ToString();
        }

        public static int GetRandomInt(int minValue = 0, int maxValue = 20)
        {
            var random = new Random((int)DateTime.Now.Ticks);
            return random.Next(minValue, maxValue<int.MaxValue? maxValue+1:int.MaxValue);
        }

        public static DateTime GetRandomDateTime()
        {
            var to = DateTime.Now;
            var from = new DateTime(1970, 1, 1);
            var range = new TimeSpan(to.Ticks - from.Ticks);
            var random = new Random((int)DateTime.Now.Ticks);
            return from + new TimeSpan((long)(range.Ticks * random.NextDouble()));
        }
    }
}
