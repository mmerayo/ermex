using System;

namespace ermeX.DAL.Interfaces.Component
{
    internal interface IRegisterComponents //TODO: sEE AUTOREGISTRATION
    {
		/// <summary>
		/// Creates a remote component and indicates if is new or it was updated
		/// </summary>
		/// <param name="remoteComponentId"></param>
		/// <param name="ip"></param>
		/// <param name="port"></param>
		/// <returns>a value indicating whether the component is new or not</returns>
        bool CreateRemoteComponent(Guid remoteComponentId, string ip, int port);
       
        void CreateLocalComponent(ushort port);
    }
}
