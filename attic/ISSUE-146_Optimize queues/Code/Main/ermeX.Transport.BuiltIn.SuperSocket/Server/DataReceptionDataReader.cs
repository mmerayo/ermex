// /*---------------------------------------------------------------------------------------*/
//        Licensed to the Apache Software Foundation (ASF) under one
//        or more contributor license agreements.  See the NOTICE file
//        distributed with this work for additional information
//        regarding copyright ownership.  The ASF licenses this file
//        to you under the Apache License, Version 2.0 (the
//        "License"); you may not use this file except in compliance
//        with the License.  You may obtain a copy of the License at
// 
//          http://www.apache.org/licenses/LICENSE-2.0
// 
//        Unless required by applicable law or agreed to in writing,
//        software distributed under the License is distributed on an
//        "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY
//        KIND, either express or implied.  See the License for the
//        specific language governing permissions and limitations
//        under the License.
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