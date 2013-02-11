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
using ProtoBuf;
using ProtoBuf.Meta;
using ermeX.Common;

namespace ermeX.LayerMessages
{
    /// <summary>
    /// Represents a message at the BizLayer level
    /// </summary>
    [ProtoContract(SkipConstructor = true)]
    internal sealed class BizMessage : SystemMessage, IEquatable<BizMessage>
    {
        private string _jsonMessage;
        private object _data = null;

        public BizMessage(object data) : base()
        {
            if (data == null) throw new ArgumentNullException("data");
            _data = data;
        }

        private BizMessage(){}

        public static BizMessage FromJson(string jsonData)
        {
            if(string.IsNullOrEmpty(jsonData)) 
                throw new ArgumentException();
            return new BizMessage(){JsonMessage = jsonData};
        }

        [ProtoMember(75)]
        internal string JsonMessage
        {
            get
            {
                if(string.IsNullOrEmpty(_jsonMessage))
                {
                    if (_data == null)
                        throw new ApplicationException(
                            "One of both, _data or the serialized json message must have a value");
                    _jsonMessage = JsonSerializer.SerializeObjectToJson(_data);
                }
                return _jsonMessage;
            }
            private set { _jsonMessage = value; }
        }

        public Type MessageType
        {
            get
            {
                UpdateData();
                if (_data == null)
                    return typeof(void);
                return _data.GetType();
            }
        }

        public object RawData
        {
            get
            {
                UpdateData();
                return _data;
            }
        }

        private void UpdateData()
        {
            if (_data == null)
            {
                if (string.IsNullOrEmpty(_jsonMessage))
                    throw new ApplicationException(
                        "One of both, _data or the serialized json message must have a value");
                _data = JsonSerializer.DeserializeObjectFromJson<object>(_jsonMessage);
            }
        }

        #region Equatable

        public bool Equals(BizMessage other)
        {
            if (other == null)
                return false;

            return JsonMessage == other.JsonMessage;
        }

        public static bool operator ==(BizMessage a, BizMessage b)
        {
            if ((object)a == null || ((object)b) == null)
                return Equals(a, b);

            return a.Equals(b);
        }

        public static bool operator !=(BizMessage a, BizMessage b)
        {
            if (a == null || b == null)
                return !Equals(a, b);

            return !(a.Equals(b));
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != typeof(BizMessage)) return false;
            return Equals((BizMessage)obj);
        }

        public override int GetHashCode()
        {
            return MessageId.GetHashCode();
        }

        #endregion
    }

}
