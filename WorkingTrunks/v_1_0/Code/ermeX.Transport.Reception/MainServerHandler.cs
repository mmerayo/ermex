// /*---------------------------------------------------------------------------------------*/
// If you viewing this code.....
// The current code is under construction.
// The reason you see this text is that lot of refactors/improvements have been identified and they will be implemented over the next iterations versions. 
// This is not a final product yet.
// /*---------------------------------------------------------------------------------------*/
using System;
using System.Diagnostics;
using ermeX.Common;
using ermeX.Transport.Interfaces.Messages;
using ermeX.Transport.Interfaces.Receiving.Server;

namespace ermeX.Transport.Reception
{
    internal class MainServerHandler : MarshalByRefObject, IServerHandlerContract
    {
        #region IServerHandlerContract Members

        public byte[] Execute(byte[] input)
        {
            object result = null;
            try
            {


                var message = ObjectSerializer.DeserializeObject<ServiceRequestMessage>(input);
                if (message.Operation == ServerBase.ChunkedMessageOperation)
                {
                    var deserializeObject =
                        ObjectSerializer.DeserializeObject<ChunkedServiceRequestMessage>(input);
                    result = OnChunkReceived(deserializeObject);
                }
                else
                {
                    result = OnRequestReceived(message);
                }
            }
            catch (Exception ex)
            {
                Debugger.Break(); //TODO LOG EXCEPTION SOMETHING HAPPEND WHILE PROCESSING REQUEST
                throw;
            }
            byte[] ret = ObjectSerializer.SerializeObjectToByteArray(result);

            return ret;
        }

        #endregion

        public event ReceptionDelegates.RequestHandler RequestReceived;
        public event ReceptionDelegates.ChunkRequestHandler ChunkReceived;

        public ServiceResult OnChunkReceived(ChunkedServiceRequestMessage message)
        {
            var handler = ChunkReceived;
            if (handler != null)
                return handler(message);
            return null;
        }

        public ServiceResult OnRequestReceived(ServiceRequestMessage request)
        {
            var handler = RequestReceived;
            if (handler != null)
                return handler(request);

            return null;
        }
    }
}