// /*---------------------------------------------------------------------------------------*/
// If you viewing this code.....
// The current code is under construction.
// The reason you see this text is that lot of refactors/improvements have been identified and they will be implemented over the next iterations versions. 
// This is not a final product yet.
// /*---------------------------------------------------------------------------------------*/
using ermeX.Entities.Entities;

namespace ermeX.Bus.Listening.Handlers.InternalMessagesHandling.Schedulers
{
    internal interface IScheduler
    {
        IncomingMessage GetNext();
    }

    //TODO:hacer que la injection carge uno u otro dependiendo de la configuracion
}