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

using Ninject;
using ermeX.Biz.Interfaces;
using ermeX.Bus.Interfaces;
using ermeX.ConfigurationManagement.Settings;
using ermeX.Logging;


namespace ermeX.Biz.Services
{
	internal class Manager : IServicesManager
	{
		[Inject]
		public Manager(IMessagePublisher publisher, IMessageListener listener,IComponentSettings settings)
		{
			Logger = LogManager.GetLogger<Manager>(settings.ComponentId,LogComponent.Messaging);

			if (publisher == null) throw new ArgumentNullException("publisher");
			if (listener == null) throw new ArgumentNullException("listener");
			Publisher = publisher;
			Listener = listener;
		}

		private IMessagePublisher Publisher { get; set; }
		private IMessageListener Listener { get; set; }
		private readonly ILogger Logger;

		#region IServicesManager Members

		public TServiceInterface GetServiceProxy<TServiceInterface>() where TServiceInterface : IService
		{
			try
			{
				return Publisher.GetServiceProxy<TServiceInterface>();
			}
			catch (Exception ex)
			{
				Logger.Error(x => x("GetServiceProxy", ex));
				throw ex;
			}
		}

		/// <summary>
		///   When there are several components publishing the same sevice, i.e. system services it specifies concretely which component to get the proxy for
		/// </summary>
		/// <typeparam name="TServiceInterface"> </typeparam>
		/// <param name="componentId"> </param>
		/// <returns> </returns>
		public TServiceInterface GetServiceProxy<TServiceInterface>(Guid componentId) where TServiceInterface : IService
		{
			try
			{
				return Publisher.GetServiceProxy<TServiceInterface>(componentId);
			}
			catch (Exception ex)
			{
				Logger.Error(x => x("GetServiceProxy", ex));
				throw ex;
			}
		}

		#endregion
	}
}