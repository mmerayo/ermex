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