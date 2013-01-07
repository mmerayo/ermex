// /*---------------------------------------------------------------------------------------*/
// If you viewing this code.....
// The current code is under construction.
// The reason you see this text is that lot of refactors/improvements have been identified and they will be implemented over the next iterations versions. 
// This is not a final product yet.
// /*---------------------------------------------------------------------------------------*/
using System;
using ermeX.Common;
using ermeX.Threading;

namespace ermeX.Bus.Listening.Handlers.InternalMessagesHandling.Workers
{
    //TODO: THIS SHOULD BE MOVED TO BIZ LAYER
    internal interface IIncomingMessagesDispatcherWorker : IWorker
    {
        event Action<Guid, object> DispatchMessage;
    }
}