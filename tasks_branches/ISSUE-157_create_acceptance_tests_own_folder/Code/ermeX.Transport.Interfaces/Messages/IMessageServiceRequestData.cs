// /*---------------------------------------------------------------------------------------*/
// If you viewing this code.....
// The current code is under construction.
// The reason you see this text is that lot of refactors/improvements have been identified and they will be implemented over the next iterations versions. 
// This is not a final product yet.
// /*---------------------------------------------------------------------------------------*/
using ermeX.LayerMessages;

namespace ermeX.Transport.Interfaces.Messages
{
    internal interface IMessageServiceRequestData : IServiceRequestMessage
    {
        TransportMessage Data { get; }
    }
}