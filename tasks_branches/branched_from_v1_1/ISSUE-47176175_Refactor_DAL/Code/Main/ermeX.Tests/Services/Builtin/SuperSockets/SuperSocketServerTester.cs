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

using ermeX.Tests.Common.SettingsProviders;
using ermeX.Tests.Services.Mock;

using ermeX.Transport.BuiltIn.SuperSocket.Server;
using ermeX.Transport.Interfaces.Entities;
using ermeX.Transport.Interfaces.Receiving.Server;

namespace ermeX.Tests.Services.Builtin.SuperSockets
{
	internal class SuperSocketServerTester : ServerTesterBase
	{
		protected override IServer GetServerInstance(ServerInfo serverInfo)
		{
			var settings =
				(TestSettingsProvider.ClientSettings) TestSettingsProvider.GetClientConfigurationSettingsSource();
			settings.MaxMessageKbBeforeChunking = 8192;
			return new SuperSocketServer(serverInfo, GetServiceDetailsReader(),
			                             GetChunkedMessagesReader(),
			                             GetChunkedMessagesWritter(),
			                             settings);
		}

		protected override IMockTestClient GetTestClientInstance(ServerInfo serverInfo)
		{
			return new DummyTestSuperSocketClient(serverInfo);
		}
	}
}