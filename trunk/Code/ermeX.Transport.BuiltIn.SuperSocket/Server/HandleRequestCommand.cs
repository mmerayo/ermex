// /*---------------------------------------------------------------------------------------*/
// If you viewing this code.....
// The current code is under construction.
// The reason you see this text is that lot of refactors/improvements have been identified and they will be implemented over the next iterations versions. 
// This is not a final product yet.
// /*---------------------------------------------------------------------------------------*/
using SuperSocket.SocketBase.Command;
using ermeX.Transport.Interfaces.Receiving.Server;

namespace ermeX.Transport.BuiltIn.SuperSocket.Server
{
    public sealed class HandleRequestCommand : CommandBase<ExposedSession, BinaryCommandInfo>
    {
        internal const string CommandName = "DR";

        public override string Name
        {
            get { return CommandName; }
        }

        public override void ExecuteCommand(ExposedSession session, BinaryCommandInfo commandInfo)
        {
            var result = ((IServerHandlerProvider) session.AppServer).ServerHandler.Execute(commandInfo.Data);
            session.SendResponse(result);
        }
    }
}