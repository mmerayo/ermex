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
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using ermeX.Common;
using ermeX.Tests.Services.Mock;
using ermeX.Transport.BuiltIn.SuperSocket.Server;
using ermeX.Transport.Interfaces.Entities;
using ermeX.Transport.Interfaces.Messages;

namespace ermeX.Tests.Services.Builtin.SuperSockets
{
    internal class DummyTestSuperSocketClient : IMockTestClient
    {
        private readonly ServerInfo _serverInfo;

        public DummyTestSuperSocketClient(ServerInfo serverInfo)
        {
            _serverInfo = serverInfo;
            if (serverInfo == null) throw new ArgumentNullException("serverInfo");
        }

        #region IMockTestClient Members

        public void Dispose()
        {
        }

        public ServiceResult Execute(byte[] msg)
        {
            string cmdHeader = AddProtocolCommandHeader(msg);

            ServiceResult result;
            using (var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
            {
                socket.Connect(new IPEndPoint(IPAddress.Parse(_serverInfo.Ip), _serverInfo.Port));
                using(var socketStream = new NetworkStream(socket))
                using (var reader = new BinaryReader(socketStream))
                using (var writer = new BinaryWriter(socketStream))
                {
                    var toSend = new List<byte>(cmdHeader.ToByteArray());
                    toSend.AddRange(msg);

                    writer.Write(toSend.ToArray());
                    writer.Flush();

                    byte[] respBytes = (reader.BaseStream as NetworkStream).ReadBytes();

                    result = ObjectSerializer.DeserializeObject<ServiceResult>(respBytes);
                }
            }
            return result;
        }

        #endregion

        private static string AddProtocolCommandHeader(byte[] msg)
        {
            string cmdHeader = HandleRequestCommand.CommandName + msg.Length.ToString().PadLeft(10, '0');
            return cmdHeader;
        }
    }
}