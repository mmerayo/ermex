using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ermeX.Configuration
{
    partial class DatabaseCallbackValidatorClass
    {
        public static void Validate(object input)
        {
            var database = input as Database;
            if(database==null)
                throw new ArgumentException("database cannot be null");

            switch(database.DbType)
            {
                case DbType.InMemory:
                    break;
                case DbType.SQLite:
                case DbType.SqlServer:
                    var db = database as PhisicalDatabase;
                    if(db.ConnectionString==string.Empty)
                        throw new ArgumentException("ConnectionString was not provided for the database");
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }

    //partial class PortNumberCallbackValidatorClass
    //{
    //    public static void Validate(object input)
    //    {
    //        var value = (int)input;
    //        if(!(value>1023 && value<ushort.MaxValue))
    //            throw new ArgumentException(string.Format("Valid ports are from 1024 to 65535. Port provided: {0}", value));
    //    }
    //}

   
}
