// /*---------------------------------------------------------------------------------------*/
// If you viewing this code.....
// The current code is under construction.
// The reason you see this text is that lot of refactors/improvements have been identified and they will be implemented over the next iterations versions. 
// This is not a final product yet.
// /*---------------------------------------------------------------------------------------*/
using System;
using ermeX.LayerMessages;
using ermeX.Transport.Interfaces.Messages;

namespace ermeX.Transport.Interfaces.Sending.Client
{
    internal interface IEndPoint : IDisposable
    {
        ServiceResult Send(ServiceRequestMessage message); //TODO: issue 16
    }
}