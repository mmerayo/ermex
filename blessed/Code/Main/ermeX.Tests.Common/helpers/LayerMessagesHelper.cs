// /*---------------------------------------------------------------------------------------*/
// If you viewing this code.....
// The current code is under construction.
// The reason you see this text is that lot of refactors/improvements have been identified and they will be implemented over the next iterations versions. 
// This is not a final product yet.
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
