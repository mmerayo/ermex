// /*---------------------------------------------------------------------------------------*/
// If you viewing this code.....
// The current code is under construction.
// The reason you see this text is that lot of refactors/improvements have been identified and they will be implemented over the next iterations versions. 
// This is not a final product yet.
// /*---------------------------------------------------------------------------------------*/
using System;

namespace ermeX.Bus.Synchronisation.Messages
{
    internal sealed class UpdatePublishedServiceMessage : DialogMessageBase
    {
        public UpdatePublishedServiceMessage(Guid sourceComponentId) : base(sourceComponentId)
        {
        }
    }
}