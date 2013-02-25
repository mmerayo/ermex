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
using Common.Logging;
using Ninject;
using ermeX.Biz.Interfaces;
using ermeX.Bus.Interfaces;
using ermeX.Common;
using ermeX.ConfigurationManagement.Settings;


namespace ermeX.Biz.Subscriptions
{
    internal sealed class Manager : ISubscriptionsManager
    {
        [Inject]
        public Manager(IMessagePublisher publisher, IMessageListener listener, IDialogsManager dialogsManager)
        {
            if (publisher == null) throw new ArgumentNullException("publisher");
            if (listener == null) throw new ArgumentNullException("listener");
            if (dialogsManager == null) throw new ArgumentNullException("dialogsManager");
            Publisher = publisher;
            Listener = listener;
            DialogsManager = dialogsManager;
        }

        private IMessagePublisher Publisher { get; set; }
        private IMessageListener Listener { get; set; }
        private IDialogsManager DialogsManager { get; set; }
        private readonly ILog Logger = LogManager.GetLogger(StaticSettings.LoggerName);

        #region ISubscriptionsManager Members

        public TResult Subscribe<TResult>(Type handlerType)
        {
            var result = Subscribe(handlerType);
            return (TResult) result;
        }


        /// <summary>
        /// Returns an array with the handlers
        /// </summary>
        /// <param name="handlerType"></param>
        /// <returns></returns>
        public object Subscribe(Type handlerType)
        {
            if (handlerType == null) throw new ArgumentNullException("handlerType");
            //ensure implementes interface
            try
            {
                Type[] genericInterfaces = TypesHelper.GetGenericInterfaces(typeof (IHandleMessages<>), handlerType);
                int count = genericInterfaces.Count();
                if (count == 0)
                    throw new ArgumentException("handlerType must implement IHandleMessages");

                if (handlerType.IsAbstract)
                    throw new ArgumentException(
                        "Message handler type must be concrete type that implements IHandleMessages");

                var result = ObjectBuilder.FromType<object>(handlerType);

                foreach (var genericInterface in genericInterfaces)
                    result = DoSubscribe(result, genericInterface);

                return result;
            }
            catch (Exception ex)
            {
                Logger.Error(x=>x( "Subscribe", ex));
                throw;
            }
        }

        public TResult Subscribe<TResult>(Type handlerType,Type messageType)
        {
            return (TResult)Subscribe(handlerType,messageType);
        }

        public object Subscribe(Type handlerType, Type messageType)
        {
            if (handlerType == null) throw new ArgumentNullException("handlerType");
            if (messageType == null) throw new ArgumentNullException("messageType");
            try
            {
                Type @interface =
                    TypesHelper.GetGenericInterfaces(typeof (IHandleMessages<>), handlerType).SingleOrDefault(
                        x => x.GetGenericArguments()[0] == messageType);
                if (@interface == null)
                    throw new ArgumentException(string.Format("handlerType must implement IHandleMessages of {0}",
                                                              messageType.FullName));

                var handlerToSuscribe = ObjectBuilder.FromType<object>(handlerType);

                handlerToSuscribe = DoSubscribe(handlerToSuscribe, @interface);
                return handlerToSuscribe;
            }
            catch (Exception ex)
            {
                Logger.Error(x=>x("Subscribe. HandlerType:{0}, MessageType:{1} Exception:{2}",handlerType.FullName,messageType.FullName, ex));
                throw;
            }
        }

        private object DoSubscribe(object handlerToSuscribe, Type interfaceHandler)
        {
            Guid subscriptionHandlerId = Listener.Suscribe(interfaceHandler, handlerToSuscribe, out handlerToSuscribe);
            DialogsManager.Suscribe(subscriptionHandlerId);
            return handlerToSuscribe;
        }
       
        #endregion
    }
}