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
    internal class DataReceptionDataReader : CommandReaderBase<BinaryCommandInfo>
    {
        private int _length;

        private string m_CommandName;

        internal void Initialize(string commandName, int length,
                                 CommandReaderBase<BinaryCommandInfo> previousCommandReader)
        {
            _length = length;
            m_CommandName = commandName;

            base.Initialize(previousCommandReader);
        }

        public override BinaryCommandInfo FindCommandInfo(IAppSession session, byte[] readBuffer, int offset, int length,
                                                          bool isReusableBuffer, out int left)
        {
            left = 0;

            int leftLength = _length - BufferSegments.Count;

            AddArraySegment(readBuffer, offset, length, isReusableBuffer);

            if (length >= leftLength)
            {
                NextCommandReader = new DataReceptionCommandReader(AppServer);
                var commandInfo = new BinaryCommandInfo(m_CommandName, BufferSegments.ToArrayData(0, _length));

                if (length > leftLength)
                    left = length - leftLength;

                return commandInfo;
            }
            else
            {
                NextCommandReader = this;
                return null;
            }
        }
    }
}