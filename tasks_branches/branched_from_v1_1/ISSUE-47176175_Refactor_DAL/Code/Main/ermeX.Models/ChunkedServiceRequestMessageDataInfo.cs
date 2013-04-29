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
using System.Data;

namespace ermeX.Models
{
    internal class ChunkedServiceRequestMessageDataInfo:ModelBase,IEquatable<ChunkedServiceRequestMessageDataInfo>
    {

        public ChunkedServiceRequestMessageDataInfo()
        {
        }
       
        public  Guid Operation { get; set; }

		//TODO:
        #region Crap due to the serializer was not supporting it, fix when supported

        public  List<byte> Data
        {
            get { return new List<byte>(DataBytes); }
            set { DataBytes = value.ToArray(); }
        }

        public  byte[] DataBytes { get; set; } 
        #endregion

        public  Guid CorrelationId { get; set; }

        public  int Order { get; set; }

        public  bool Eof { get; set; }

        #region Equatable

        public  bool Equals(ChunkedServiceRequestMessageDataInfo other)
        {
            if (other == null)
                return false;

            return Operation == other.Operation
                && CorrelationId == other.CorrelationId
                && Order == other.Order
                && Version == other.Version
                && Eof == other.Eof;
        }

        public static bool operator ==(ChunkedServiceRequestMessageDataInfo a, ChunkedServiceRequestMessageDataInfo b)
        {
            if ((object)a == null || ((object)b) == null)
                return Equals(a, b);

            return a.Equals(b);
        }

        public static bool operator !=(ChunkedServiceRequestMessageDataInfo a, ChunkedServiceRequestMessageDataInfo b)
        {
            if (a == null || b == null)
                return !Equals(a, b);

            return !(a.Equals(b));
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != typeof(ChunkedServiceRequestMessageDataInfo)) return false;
            return Equals((ChunkedServiceRequestMessageDataInfo)obj);
        }

        public override int GetHashCode()
        {
            return CorrelationId.GetHashCode()^Order.GetHashCode();
        }

        #endregion

        public static ChunkedServiceRequestMessageDataInfo NewFromExisting(ChunkedServiceRequestMessageDataInfo source)
        {
            var result = new ChunkedServiceRequestMessageDataInfo()
            {
                OwnedBy = source.OwnedBy,
                CorrelationId=source.CorrelationId,
                Data = source.Data,
                Eof=source.Eof,
                Operation = source.Operation,
                Order=source.Order
            };
            return result;
        }

	   
    }
}
