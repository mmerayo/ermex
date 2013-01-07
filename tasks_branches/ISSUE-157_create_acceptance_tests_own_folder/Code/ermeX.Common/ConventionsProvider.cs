// /*---------------------------------------------------------------------------------------*/
// If you viewing this code.....
// The current code is under construction.
// The reason you see this text is that lot of refactors/improvements have been identified and they will be implemented over the next iterations versions. 
// This is not a final product yet.
// /*---------------------------------------------------------------------------------------*/
using System;

namespace ermeX.Common
{
    public static class ConventionsProvider
    {
        //TODO: EStablish convention
        public static DateTime? GetDateTime(long ticks)
        {
            return new DateTime(ticks);
        }

        public static long GetTicksDateTime(DateTime source)
        {
            return source.Ticks;
        }
    }
}