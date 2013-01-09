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
using System;
using System.Text;
using SuperSocket.Common;
using SuperSocket.SocketBase;
using SuperSocket.SocketBase.Command;
using SuperSocket.SocketBase.Protocol;

namespace ermeX.Transport.BuiltIn.SuperSocket.Server
{
    internal class DataReceptionCommandReader : CommandReaderBase<BinaryCommandInfo>
    {
        private readonly DataReceptionDataReader _preparedDataReader = new DataReceptionDataReader();

        public DataReceptionCommandReader(IAppServer appServer) : base(appServer)
        {
        }

        private DataReceptionDataReader GetDataReader(string commandName, int dataLength)
        {
            _preparedDataReader.Initialize(commandName, dataLength, this);
            return _preparedDataReader;
        }

        /// <summary>
        ///   Finds the command. "DR0000000008xg^89W(v" Read 12 chars as command name and command data length
        /// </summary>
        /// <param name="session"> The session. </param>
        /// <param name="readBuffer"> The read buffer. </param>
        /// <param name="offset"> The offset. </param>
        /// <param name="length"> The length. </param>
        /// <param name="isReusableBuffer"> if set to <c>true</c> [is reusable buffer]. </param>
        /// <returns> </returns>
        public override BinaryCommandInfo FindCommandInfo(IAppSession session, byte[] readBuffer, int offset, int length,
                                                          bool isReusableBuffer, out int left)
        {
            left = 0;

            const int dataStart = 12;
            const int cmdNameLen = 2;
            const int cmdLenStart = 2;
            const int cmdLenLen = 10;

            int leftLength = dataStart - BufferSegments.Count;

            if (length < leftLength)
            {
                AddArraySegment(readBuffer, offset, length, isReusableBuffer);
                NextCommandReader = this;
                return null;
            }

            AddArraySegment(readBuffer, offset, leftLength, isReusableBuffer);

            string commandName = BufferSegments.Decode(Encoding.UTF8, 0, cmdNameLen);

            int commandDataLength =
                Convert.ToInt32(BufferSegments.Decode(Encoding.UTF8, cmdLenStart, cmdLenLen).TrimStart('0'));

            if (length > leftLength)
            {
                int leftDataLength = length - leftLength;
                if (leftDataLength >= commandDataLength)
                {
                    byte[] commandData = readBuffer.CloneRange(offset + leftLength, commandDataLength);
                    BufferSegments.ClearSegements();
                    NextCommandReader = this;
                    var commandInfo = new BinaryCommandInfo(commandName, commandData);

                    //The next commandInfo is comming
                    if (leftDataLength > commandDataLength)
                        left = leftDataLength - commandDataLength;

                    return commandInfo;
                }
                else // if (leftDataLength < commandDataLength)
                {
                    //Clear previous cached header data
                    BufferSegments.ClearSegements();
                    //Save left data part
                    AddArraySegment(readBuffer, offset + leftLength, length - leftLength, isReusableBuffer);
                    NextCommandReader = GetDataReader(commandName, commandDataLength);
                    return null;
                }
            }
            else
            {
                NextCommandReader = GetDataReader(commandName, commandDataLength);
                return null;
            }
        }
    }
}