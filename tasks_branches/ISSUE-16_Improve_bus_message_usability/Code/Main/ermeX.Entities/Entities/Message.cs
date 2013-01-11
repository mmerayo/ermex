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
using ermeX.Entities.Base;
using ermeX.LayerMessages;

namespace ermeX.Entities.Entities
{
    //TODO: THERES LOTS OF CRAP HERE THAT need to be refactored
    [Serializable]
    internal abstract class Message : ModelBase
    {

        protected Message()
        {
        }

        protected Message(BusMessage message)
        {
            if (message == null) throw new ArgumentNullException("message");
            //TODO: REMOVE, AS the biz message should be handled only by the bizlayer
            BusMessage = message;
            TimePublishedUtc = message.CreatedTimeUtc;
        }

        protected abstract string TableName { get; }


        private BusMessageData _busMessageData = null;

        public virtual BusMessage BusMessage
        {
            get { return _busMessageData; }
            set { _busMessageData = BusMessageData.FromBusLayerMessage(ComponentOwner, value); }
        }

        //TODO: MOVE THE STATUS TO and independent container
        public virtual BusMessageData.BusMessageStatus Status
        {
            get { return _busMessageData.Status; }
            set { _busMessageData.Status = value; }
        }

        public virtual DateTime TimePublishedUtc { get; set; }


        public virtual Guid PublishedBy
        {
            get { return ComponentOwner; }
            set { ComponentOwner = value; }
        }

        //TODO: to compenentData object when provider specified

        public virtual Guid PublishedTo { get; set; } //TODO: to compenentData object when provider specified


        protected string GetDbFieldName(string fieldName)
        {
            return string.Format("{0}_{1}", TableName, fieldName);
        }
    }
}