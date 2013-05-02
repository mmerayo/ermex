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
using System.Net.Sockets;
using Common.Logging;
using ermeX.Common;
using ermeX.Common.Caching;
using ermeX.ConfigurationManagement.Settings;
using ermeX.DAL.Interfaces;
using ermeX.DAL.Interfaces.Connectivity;
using ermeX.Models.Entities;
using ermeX.Transport.Interfaces.Entities;
using ermeX.Transport.Interfaces.Receiving.Server;
using ermeX.Transport.Interfaces.Sending.Client;
using ermeX.Transport.LayerInterfaces;

namespace ermeX.Transport
{
	internal abstract class LoaderBase : IConcreteServiceLoader
	{
		private static volatile List<IServer> _servers;
		private static volatile object _locker = new object();

		protected LoaderBase(ITransportSettings settings, ICanReadConnectivityDetails connectivityReader,
		                     ICanWriteConnectivityDetails connectivityWritter,
		                     ICacheProvider cacheProvider)
		{
			if (settings == null) throw new ArgumentNullException("settings");
			if (cacheProvider == null) throw new ArgumentNullException("cacheProvider");
			Settings = settings;
			ConnectivityDetailsReader = connectivityReader;
			ConnectivityDetailsWritter = connectivityWritter;
			CacheProvider = cacheProvider;
		}

		protected ITransportSettings Settings { get; set; }
		protected ICanReadConnectivityDetails ConnectivityDetailsReader { get; set; }
		private ICanWriteConnectivityDetails ConnectivityDetailsWritter { get; set; }
		protected ICacheProvider CacheProvider { get; set; }
		protected static readonly ILog Logger = LogManager.GetLogger(typeof(LoaderBase).FullName);

		#region IConcreteServiceLoader Members

		/// <summary>
		///   It generates a new set of IP:port when loaded first time
		/// </summary>
		/// <returns> </returns>
		public IEnumerable<IServer> LoadServers()
		{
			lock (_locker)
			{
				if (_servers == null)
					_servers = new List<IServer>();

				var endPoint = ConnectivityDetailsReader.Fetch(Settings.ComponentId);
				if (endPoint == null)
				{
					CreateConnectivitySet();
					endPoint = ConnectivityDetailsReader.Fetch(Settings.ComponentId);
				}
				_servers.Add(GetServer(endPoint));
			}

			return _servers;
		}

		public IEndPoint GetClientProxy(Guid destinationComponent)
		{
			var connectivityDetails = ConnectivityDetailsReader.Fetch(destinationComponent);
			var serverInfos = new List<ServerInfo>();


			serverInfos.Add(new ServerInfo
				{
					Ip = connectivityDetails.Ip,
					Port = connectivityDetails.Port,
					ServerId = connectivityDetails.ServerId
				});
			return GetClientConcreteProxy(destinationComponent, serverInfos);
		}

		#endregion

		private void CreateConnectivitySet()
		{
			ConnectivityDetailsWritter.RemoveComponentDetails(Settings.ComponentId);

			//local
			// CreateConnectivityDetails(IPAddress.Loopback.ToString(),true);

			//IP4
			ConnectivityDetailsWritter.CreateComponentConnectivityDetails(Settings.TcpPort, asLocal: false);
		}

		protected abstract IServer GetServer(ConnectivityDetails item);

		protected abstract int GetPort(ConnectivityDetails item);
		protected abstract IEndPoint GetClientConcreteProxy(Guid serverId, List<ServerInfo> serverInfos);

		#region IDisposable

		private bool _disposed;

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected void Dispose(bool disposing)
		{
			if (disposing)
			{
			}
			if (_servers != null)
				while (_servers.Count > 0)
				{
					var server = _servers[0];
					_servers.RemoveAt(0);
					server.Dispose();
				}
			_disposed = true;
		}

		~LoaderBase()
		{
			Dispose(false);
		}

		#endregion
	}
}