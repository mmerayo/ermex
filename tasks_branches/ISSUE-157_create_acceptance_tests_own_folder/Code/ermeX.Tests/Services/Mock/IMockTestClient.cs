// /*---------------------------------------------------------------------------------------*/
// If you viewing this code.....
// The current code is under construction.
// The reason you see this text is that lot of refactors/improvements have been identified and they will be implemented over the next iterations versions. 
// This is not a final product yet.
// /*---------------------------------------------------------------------------------------*/
using System;
using ermeX.Transport.Interfaces.Messages;

namespace ermeX.Tests.Services.Mock
{
    internal interface IMockTestClient : IDisposable
    {
        ServiceResult Execute(byte[] msg);
    }
}