// /*---------------------------------------------------------------------------------------*/
// If you viewing this code.....
// The current code is under construction.
// The reason you see this text is that lot of refactors/improvements have been identified and they will be implemented over the next iterations versions. 
// This is not a final product yet.
// /*---------------------------------------------------------------------------------------*/
using System;

namespace ermeX.DAL.Interfaces
{
    /// <summary>
    /// Registers items that dont need to be exhanged. The system items
    /// </summary>
    internal interface IAutoRegistration
    {
        bool CreateRemoteComponentInitialSetOfData(Guid remoteComponentId, string ip, int port);
        //void RegisterRemoteSystemServices(Type typeService, Guid remoteComponentId);

        //void RegisterRemoteSystemService(Guid componentId, string serviceImplementationMethodName,Type serviceInterfaceType);

        /// <summary>
        ///   creates the initial set of data
        /// </summary>
        void CreateLocalSetOfData(int port);
    }
}
