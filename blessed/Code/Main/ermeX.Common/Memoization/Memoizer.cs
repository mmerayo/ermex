using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace ermeX.Common.Memoization
{
    public class Memoizer
    {
        public static Func<TReturn> Memoize<TReturn>(Func<TReturn> func)
        {
            object resultCache = null;
            return () =>
            {
                if (resultCache == null)
                    resultCache = func();

                return (TReturn)resultCache;
            };
        }
        public static Func<TSource, TReturn> Memoize<TSource, TReturn>(Func<TSource, TReturn> func)
        {
            var resultCache = new Dictionary<TSource, TReturn>();
            return s =>
            {
                if (!resultCache.ContainsKey(s))
                {
                    resultCache[s] = func(s);
                }
                return resultCache[s];
            };
        }
        public static Func<TSource1, TSource2, TReturn> Memoize<TSource1, TSource2, TReturn>(Func<TSource1, TSource2, TReturn> func)
        {
            var resultCache = new Dictionary<string, TReturn>();
            return (arg1, arg2) =>
            {
                var key = arg1.GetHashCode().ToString(CultureInfo.InvariantCulture) + arg2.GetHashCode().ToString(CultureInfo.InvariantCulture);
                if (!resultCache.ContainsKey(key))
                {
                    resultCache[key] = func(arg1, arg2);
                }
                return resultCache[key];
            };
        }


        public static Func<TSource1, TSource2, TSource3, TReturn> Memoize<TSource1, TSource2, TSource3, TReturn>(Func<TSource1, TSource2, TSource3, TReturn> func)
        {
            var resultCache = new Dictionary<string, TReturn>();
            return (arg1, arg2,arg3) =>
                {
                    var key = arg1.GetHashCode().ToString(CultureInfo.InvariantCulture) +
                              arg2.GetHashCode().ToString(CultureInfo.InvariantCulture) +
                              arg3.GetHashCode().ToString(CultureInfo.InvariantCulture);
                if (!resultCache.ContainsKey(key))
                {
                    resultCache[key] = func(arg1, arg2, arg3);
                }
                return resultCache[key];
            };
        }

        //TODO: extend on demand
    }
}
