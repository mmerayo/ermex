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
using ermeX.LayerMessages;

namespace ermeX.Tests.Common.Helpers
{
    public static class LayerMessagesHelper
    {
        public enum LayerMessageType
        {
            Biz, Bus, Transport
        }

        public static TResult GetLayerMessage<TData, TResult>(TData data)
        {
            LayerMessageType type=LayerMessageType.Transport;
            if(typeof(TResult)==typeof(BizMessage))
                type=LayerMessageType.Biz;
            else if (typeof(TResult) == typeof(BusMessage))
                type = LayerMessageType.Bus;
            return (TResult)GetLayerMessage(type, data);
        }

        public static TResult GetLayerMessage<TData,TResult>(LayerMessageType messageType, TData data)
        {
            return (TResult)GetLayerMessage(messageType,data);
        }

        public static object GetLayerMessage<TData>(LayerMessageType messageType, TData data)
        {
            object result = new BizMessage(data);
            if (messageType == LayerMessageType.Bus || messageType == LayerMessageType.Transport)
                result = new BusMessage(Guid.NewGuid(), (BizMessage)result);
            if (messageType == LayerMessageType.Transport)
                result = new TransportMessage(Guid.NewGuid(), (BusMessage)result);
            return result;
        }
    }
}
