// /*---------------------------------------------------------------------------------------*/
//        Licensed to the Apache Software Foundation (ASF) under one
//        or more contributor license agreements.  See the NOTICE file
//        distributed with this work for additional information
//        regarding copyright ownership.  The ASF licenses this file
//        to you under the Apache License, Version 2.0 (the
//        "License"); you may not use this file except in compliance
//        with the License.  You may obtain a copy of the License at
// 
//          http://www.apache.org/licenses/LICENSE-2.0
// 
//        Unless required by applicable law or agreed to in writing,
//        software distributed under the License is distributed on an
//        "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY
//        KIND, either express or implied.  See the License for the
//        specific language governing permissions and limitations
//        under the License.
// /*---------------------------------------------------------------------------------------*/
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
