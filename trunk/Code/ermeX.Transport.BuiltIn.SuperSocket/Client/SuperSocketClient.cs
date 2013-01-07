// /*---------------------------------------------------------------------------------------*/
// If you viewing this code.....
// The current code is under construction.
// The reason you see this text is that lot of refactors/improvements have been identified and they will be implemented over the next iterations versions. 
// This is not a final product yet.
// /*---------------------------------------------------------------------------------------*/
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using ermeX.Common;
using ermeX.Common.Caching;
using ermeX.ConfigurationManagement.Settings;
using ermeX.Transport.BuiltIn.SuperSocket.Server;
using ermeX.Transport.Interfaces.Entities;
using ermeX.Transport.Interfaces.Messages;
using ermeX.Transport.Publish;

namespace ermeX.Transport.BuiltIn.SuperSocket.Client
{
    internal sealed class SuperSocketClient : ServiceClientBase<SuperSocketClient>
    {
        public SuperSocketClient(ICacheProvider cacheProvider, ITransportSettings settings,
                                 List<ServerInfo> serverInfos) : base(cacheProvider, settings, serverInfos)
        {
        }

        protected override ServiceResult DoSend(ServiceRequestMessage message, SuperSocketClient client)
        {
            return DoSend(message, (object) client);
        }

        protected override SuperSocketClient GetClientInstance()
        {
            return this;
        }

        protected override void SetUpCurrentServer()
        {
        }

        protected override byte[] DoConcreteSend(object proxy, byte[] msg)
        {
            byte[] respBytes;
            using (var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
            {
                socket.Connect(new IPEndPoint(IPAddress.Parse(_server.Ip), _server.Port));
                using (var socketStream = new NetworkStream(socket))

                    respBytes = SendMsg(msg, socketStream);
            }

            return respBytes;
        }


        protected override byte[] DoConcreteSend(object proxy, List<byte[]> chunks)
        {
            var respBytes = new byte[0];

            foreach (var chunk in chunks)
                using (var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
                {
                    socket.Connect(new IPEndPoint(IPAddress.Parse(_server.Ip), _server.Port));
                    using (var socketStream = new NetworkStream(socket))
                    {
                        respBytes = SendMsg(chunk, socketStream);
                    }
                }

            return respBytes;
        }

        private static byte[] SendMsg(byte[] msg, Stream socketStream)
        {
            byte[] respBytes;
            using (var reader = new BinaryReader(socketStream))
            using (var writer = new BinaryWriter(socketStream))
            {
                var cmdHeader = AddProtocolCommandHeader(msg);

                var toSend = new List<byte>(cmdHeader.ToByteArray());
                toSend.AddRange(msg);

                writer.Write(toSend.ToArray());
                writer.Flush();

                respBytes = (reader.BaseStream as NetworkStream).ReadBytes();
            }
            return respBytes;
        }

        private static string AddProtocolCommandHeader(byte[] msg)
        {
            var cmdHeader = HandleRequestCommand.CommandName + msg.Length.ToString().PadLeft(10, '0');
            return cmdHeader;
        }
    }
}