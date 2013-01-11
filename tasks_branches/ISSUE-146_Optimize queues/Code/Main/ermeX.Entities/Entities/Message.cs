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
using ermeX.Common;
using ermeX.Entities.Base;
using ermeX.LayerMessages;

namespace ermeX.Entities.Entities
{
    //TODO: THERES LOTS OF CRAP HERE THAT need to be refactored
    [Serializable]
    internal abstract class Message : ModelBase
    {
        public enum MessageStatus : int
        {
            NotSet = 0,


            /// <summary>
            /// This is an special stutus to save the first stage, no copies created per subscriber yet
            /// </summary>
            SenderCollected = 1,
            /// <summary>
            /// Is ready to deliver to subscriber. Its refered by outpoing message
            /// </summary>
            SenderDispatchPending = 2,
            /// <summary>
            /// Marks the message as sent
            /// </summary>
            SenderSent,

            /// <summary>
            /// Received but not created copy per local subscription
            /// </summary>
            ReceiverReceived,

            /// <summary>
            /// Ready to be delivered to the handler
            /// </summary>
            ReceiverDispatchable,

            /// <summary>
            /// its being dispatched now
            /// </summary>
            ReceiverDispatching
        }

        protected Message()
        {
        }

        protected Message(BusMessage message)
        {
            if (message == null) throw new ArgumentNullException("message");
            //TODO: REMOVE, AS the biz message should be handled only by the bizlayer
            PublishedBy= message.Publisher;
            JsonMessage = message.Data.JsonMessage;
            MessageId = message.MessageId;
            CreatedTimeUtc = message.CreatedTimeUtc;
        }

        protected abstract string TableName { get; }

        public virtual MessageStatus Status { get; set; }

        public virtual string JsonMessage { get; set; }

        public virtual Guid MessageId { get; set; }

        public virtual DateTime CreatedTimeUtc { get; set; }

        public virtual Guid PublishedBy
        {
            get { return ComponentOwner; }
            set { ComponentOwner = value; }
        }

        public virtual BusMessage ToBusMessage()
        {
            BizMessage bizMessage = BizMessage.FromJson(JsonMessage);
            return new BusMessage(MessageId,CreatedTimeUtc,PublishedBy,bizMessage);
        }

        //TODO: to compenentData object when provider specified

        public virtual Guid PublishedTo { get; set; } //TODO: to compenentData object when provider specified


        protected string GetDbFieldName(string fieldName)
        {
            return String.Format("{0}_{1}", TableName, fieldName);
        }

        
    }
}