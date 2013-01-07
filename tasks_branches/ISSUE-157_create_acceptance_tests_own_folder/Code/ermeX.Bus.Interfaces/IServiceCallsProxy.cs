// /*---------------------------------------------------------------------------------------*/
// If you viewing this code.....
// The current code is under construction.
// The reason you see this text is that lot of refactors/improvements have been identified and they will be implemented over the next iterations versions. 
// This is not a final product yet.
// /*---------------------------------------------------------------------------------------*/
using System;

namespace ermeX.Bus.Interfaces
{
    internal interface IServiceCallsProxy
    {
        /// <summary>
        ///   Used for when there are multiple components exposing the same service i.e. system services
        /// </summary>
        /// <param name="componentId"> </param>
        void SetDestinationComponent(Guid componentId);
    }
}