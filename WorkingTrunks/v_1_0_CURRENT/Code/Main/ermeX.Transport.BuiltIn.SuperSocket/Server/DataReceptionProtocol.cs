// /*---------------------------------------------------------------------------------------*/
// If you viewing this code.....
// The current code is under construction.
// The reason you see this text is that lot of refactors/improvements have been identified and they will be implemented over the next iterations versions. 
// This is not a final product yet.
// /*---------------------------------------------------------------------------------------*/
using SuperSocket.SocketBase;
using SuperSocket.SocketBase.Command;
using SuperSocket.SocketBase.Protocol;

namespace ermeX.Transport.BuiltIn.SuperSocket.Server
{
    /// <summary>
    ///   It's a protocol like that "DR0000000008xg^89W(v" "DR" is command name, which is 2 chars "0000000008" is command data length, which is 10 chars(int.MaxValue) "xg^89W(v" is the command data whose lenght is 8
    /// </summary>
    internal class DataReceptionProtocol : ICustomProtocol<BinaryCommandInfo>
    {
        #region ICustomProtocol<BinaryCommandInfo> Members

        public ICommandReader<BinaryCommandInfo> CreateCommandReader(IAppServer appServer)
        {
            return new DataReceptionCommandReader(appServer);
        }

        #endregion
    }
}