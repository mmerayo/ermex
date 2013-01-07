// /*---------------------------------------------------------------------------------------*/
// If you viewing this code.....
// The current code is under construction.
// The reason you see this text is that lot of refactors/improvements have been identified and they will be implemented over the next iterations versions. 
// This is not a final product yet.
// /*---------------------------------------------------------------------------------------*/
using System;
using SuperSocket.SocketBase;
using SuperSocket.SocketBase.Command;

namespace ermeX.Transport.BuiltIn.SuperSocket.Server
{
    public sealed class ExposedSession : AppSession<ExposedSession, BinaryCommandInfo>
    {
        public override void HandleExceptionalError(Exception e)
        {
        }
    }
}