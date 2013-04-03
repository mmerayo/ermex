using System;
using System.Collections;
using System.Web;

namespace ermeX.DAL.DataAccess.UoW
{
	public static class Local
	{
		private static readonly ILocalData _data = new LocalData();

		public static ILocalData Data
		{
			get { return _data; }
		}

		private class LocalData : ILocalData
		{
			[ThreadStatic] //this makes it thread-safe
			private static Hashtable _localData;
			private static readonly object LocalDataHashtableKey = new object();

			private static Hashtable LocalHashtable
			{
				get
				{
					Hashtable result;

					if (!RunningInWeb)
					{
						if (_localData == null)
							_localData = new Hashtable();
						result = _localData;
					}
					else
					{
						var web_hashtable = HttpContext.Current.Items[LocalDataHashtableKey] as Hashtable;
						if (web_hashtable == null)
						{
							web_hashtable = new Hashtable();
							HttpContext.Current.Items[LocalDataHashtableKey] = web_hashtable;
						}
						result = web_hashtable;
					}
					return result;
				}
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

			private static bool RunningInWeb
			{
				get { return HttpContext.Current != null; }
			}
		}
	}
}