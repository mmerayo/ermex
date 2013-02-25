using System;
using System.Collections;
using ermeX.DAL.Interfaces.UnitOfWork;

namespace ermeX.DAL.DataAccess.UnitOfWork
{
    internal static class Local
    {
        static readonly ILocalData _data = new LocalData();

        public static ILocalData Data
        {
            get { return _data; }
        }

        private class LocalData : ILocalData
        {
            [ThreadStatic]
            private static Hashtable _localData;

            private static Hashtable LocalHashtable
            {
                get { return _localData ?? (_localData = new Hashtable()); }
            }

            public object this[object key]
            {
                get { return LocalHashtable[key]; }
                set { LocalHashtable[key] = value; }
            }

            public int Count
            {
                get { return LocalHashtable.Count; }
            }

            public void Clear()
            {
                LocalHashtable.Clear();
            }
        }
    }
}