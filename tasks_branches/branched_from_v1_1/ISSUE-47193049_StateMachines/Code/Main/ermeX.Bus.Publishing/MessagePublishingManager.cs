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
using System.Linq;
using Castle.DynamicProxy;
using Ninject;
using ermeX.Bus.Interfaces;
using ermeX.Common;
using ermeX.ConfigurationManagement.IoC;
using ermeX.DAL.Interfaces;
using ermeX.DAL.Interfaces.Services;
using ermeX.LayerMessages;

namespace ermeX.Bus.Publishing
{
	internal class MessagePublishingManager : BusInteropBase, IMessagePublisher
	{
		private readonly ICanReadServiceDetails _serviceDetailsReader;

		[Inject]
		public MessagePublishingManager(IEsbManager bus,
		                                ICanReadServiceDetails serviceDetailsReader)
			: base(bus)
		{
			_serviceDetailsReader = serviceDetailsReader;
		}

		#region IMessagePublisher Members

		public void PublishMessage(BusMessage message)
		{
			Bus.Publish(message);
		}

		public TServiceInterface GetServiceProxy<TServiceInterface>() where TServiceInterface : IService
		{
			return GetServiceProxy<TServiceInterface>(Guid.Empty);
		}

		//when the service is exposed by several components it specifies the concrete one
		public TServiceInterface GetServiceProxy<TServiceInterface>(Guid componentId) where TServiceInterface : IService
		{
			var operations = _serviceDetailsReader.GetByInterfaceType(typeof (TServiceInterface));
			if (!operations.Any())
				return default(TServiceInterface);

			//TODO: REPLACE in the cases THERE ARE REAL local implementations instead of the PROXIES
			var proxy = IoCManager.Kernel.Get<IServiceCallsProxy>();

			if (!componentId.IsEmpty())
				proxy.SetDestinationComponent(componentId);

			var obj = ObjectBuilder.CreateProxy<TServiceInterface>((IInterceptor) proxy);
			return obj;
		}

		#endregion

		public void Stop()
		{
			throw new NotImplementedException();
		}
	}
}