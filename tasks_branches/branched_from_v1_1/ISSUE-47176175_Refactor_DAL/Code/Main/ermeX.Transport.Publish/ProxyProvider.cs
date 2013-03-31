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
using Ninject;
using ermeX.ConfigurationManagement.Settings;
using ermeX.DAL.Interfaces;
using ermeX.Transport.Interfaces.Sending.Client;
using ermeX.Transport.LayerInterfaces;

namespace ermeX.Transport.Publish
{
    internal class ProxyProvider : IProxyProvider
    {
        [Inject]
        public ProxyProvider(ITransportSettings settings,
                             IEnumerable<IConcreteServiceLoader> serviceLoaders)
        {
            if (settings == null) throw new ArgumentNullException("settings");
            if (serviceLoaders == null) throw new ArgumentNullException("serviceLoaders");
            Settings = settings;
            ServiceLoaders = serviceLoaders;
        }

        private IEnumerable<IConcreteServiceLoader> ServiceLoaders { get; set; }

        private ITransportSettings Settings { get; set; }

        #region IProxyProvider Members

        public List<IEndPoint> GetClientProxies(Guid destinationComponent)
        {
            var result = new List<IEndPoint>();
            foreach (var concreteServiceLoader in ServiceLoaders)
            {
                result.Add(concreteServiceLoader.GetClientProxy(destinationComponent));
            }

            return result;
        }

        #endregion
    }
}