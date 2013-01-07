// /*---------------------------------------------------------------------------------------*/
// If you viewing this code.....
// The current code is under construction.
// The reason you see this text is that lot of refactors/improvements have been identified and they will be implemented over the next iterations versions. 
// This is not a final product yet.
// /*---------------------------------------------------------------------------------------*/
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Common.Logging;
using Ninject;
using ermeX.Bus.Interfaces;
using ermeX.Bus.Interfaces.Attributes;
using ermeX.Common;
using ermeX.ConfigurationManagement.IoC;
using ermeX.ConfigurationManagement.Settings;
using ermeX.ConfigurationManagement.Settings.Component;
using ermeX.DAL.Interfaces;

using ermeX.Entities.Entities;
using ermeX.Interfaces;

namespace ermeX.Bus.Listening
{
    //TODO: prevalece siempre governance settings MESSAGE PRIORITY. Manejar en la recepcion de suscribers fijarse tb en version
    internal sealed class MessageListeningManager : IMessageListener, IDisposable
    {
        private readonly IDictionary<Guid, object> _suscriptions = new ConcurrentDictionary<Guid, object>();
        private bool _started;

        [Inject]
        public MessageListeningManager(IIncomingMessageSuscriptionsDataSource suscriptionsDatasource,
                                       IOutgoingMessageSuscriptionsDataSource outgoingSubscriptionsDatasource,
                                       IComponentSettings componentSettings, IListeningManager listeningManager,
                                       IServiceDetailsDataSource serviceDetailsDataSource
            
            )
        {
            if (suscriptionsDatasource == null) throw new ArgumentNullException("suscriptionsDatasource");
            if (outgoingSubscriptionsDatasource == null)
                throw new ArgumentNullException("outgoingSubscriptionsDatasource");
            if (componentSettings == null) throw new ArgumentNullException("componentSettings");
            if (listeningManager == null) throw new ArgumentNullException("listeningManager");
            if (serviceDetailsDataSource == null) throw new ArgumentNullException("serviceDetailsDataSource");
            SuscriptionsDatasource = suscriptionsDatasource;
            OutgoingSubscriptionsDatasource = outgoingSubscriptionsDatasource;
            ComponentSettings = componentSettings;
            ListeningManager = listeningManager;
            ServiceDetailsDataSource = serviceDetailsDataSource;
            RemoveSystemSubscriptions();
        }

        private IIncomingMessageSuscriptionsDataSource SuscriptionsDatasource { get; set; }
        private IOutgoingMessageSuscriptionsDataSource OutgoingSubscriptionsDatasource { get; set; }

        private IComponentSettings ComponentSettings { get; set; }
        private IListeningManager ListeningManager { get; set; }
        private IServiceDetailsDataSource ServiceDetailsDataSource { get; set; }
        private readonly ILog Logger=LogManager.GetLogger(StaticSettings.LoggerName);

        #region IMessageListener Members

        public Guid Suscribe(Type handlerInterfaceType, object handler)
        {
            object outHandler;
            return Suscribe(handlerInterfaceType, handler, out outHandler);
        }

        public Guid Suscribe(Type handlerInterfaceType, object inhandler, out object outHandler)
        {
            if (handlerInterfaceType == null) throw new ArgumentNullException("handlerInterfaceType");
            IncomingMessageSuscription subscription = SuscriptionsDatasource.GetByHandlerAndMessageType(inhandler.GetType(),handlerInterfaceType.GetGenericArguments()[0]);
            Guid suscriptionHandlerId = subscription == null ? Guid.NewGuid() : subscription.SuscriptionHandlerId;

            if (subscription == null)
            {
                AddToHandlers(suscriptionHandlerId, inhandler);

                SaveSuscription(handlerInterfaceType, suscriptionHandlerId, inhandler.GetType());
            }

            outHandler = _suscriptions[suscriptionHandlerId];

            return suscriptionHandlerId;
        }

        public void Start()
        {
            lock (this)
            {
                if (!_started)
                {
                    ListeningManager.StartServers(OnDispatchMessage);
                    RestoreMessageHandlers();
                    _started = true;
                }
            }
        }


        public void PublishService<TServiceInterface>(Type serviceImplementationType) where TServiceInterface : IService
        {
            PublishService(typeof (TServiceInterface), serviceImplementationType);

        }

        public void PublishService<TServiceInterface>() where TServiceInterface : IService
        {
            PublishService<TServiceInterface>(IoCManager.Kernel.Get<TServiceInterface>().GetType());
        }

        public void PublishService(Type serviceInterface, Type serviceImplementation)
        {
            if (serviceInterface == null) throw new ArgumentNullException("serviceInterface");
            if (serviceImplementation == null) throw new ArgumentNullException("serviceImplementation");

            SavePublishedService(serviceInterface,serviceImplementation);

            //TODO: send events
            Logger.Trace(x=>x("Published Service:{0} Component:{1}", serviceInterface.FullName,ComponentSettings.ComponentId));

        }

        #endregion

        protected void RemoveSystemSubscriptions()
        {
            //TODO: IMPROVE THIS MECHNISM TO DETECT INTERNAL HANDLERS
            var internalHandlerNames = new[] {"UpdatePublishedServiceMessageHandler", "UpdateSuscriptionMessageHandler"};
            var incomingMessageSuscriptions = SuscriptionsDatasource.GetAll();
            var toRemove = new List<IncomingMessageSuscription>();
            foreach (var incomingMessageSuscription in incomingMessageSuscriptions)
            {
                string typeName = incomingMessageSuscription.HandlerType.Split('.').Last();
                if (internalHandlerNames.Contains(typeName))
                    toRemove.Add(incomingMessageSuscription);
            }

            SuscriptionsDatasource.Remove(toRemove);
        }

        private void RestoreMessageHandlers()
        {
            //TODO: IMPROVE THIS MECHNISM TO DETECT INTERNAL HANDLERS
            var internalHandlerNames = new[] {"UpdatePublishedServiceMessageHandler", "UpdateSuscriptionMessageHandler"};

            //TODO: this code need a big refactor(Whole app)
            var incomingMessageSuscriptions = SuscriptionsDatasource.GetAll();
            foreach (var incomingMessageSuscription in incomingMessageSuscriptions)
            {
                string typeName = incomingMessageSuscription.HandlerType.Split('.').Last();
                if (!internalHandlerNames.Contains(typeName))
                {
                    Type handlerType = TypesHelper.GetTypeFromDomain(incomingMessageSuscription.HandlerType);
                    AddToHandlers(incomingMessageSuscription.SuscriptionHandlerId,
                                  ObjectBuilder.FromType<object>(handlerType));
                }
            }
        }

        private void OnDispatchMessage(Guid suscriptionId, object message)
        {
            if (message == null) throw new ArgumentNullException("message");
            Logger.Trace("MessageListeningManager.OnDispatchMessage");

            try
            {
                if (_suscriptions.ContainsKey(suscriptionId))
                {
                    //TODO: IMPROVE THE PERFORMANCE OF THIS
                    object handler = _suscriptions[suscriptionId];
                    Type type = handler.GetType();

                    //invoke dispatch
                    var dispatchMethodInfos = TypesHelper.GetPublicInstanceMethods(type, "HandleMessage",
                                                                                   message.GetType());
                    if(!dispatchMethodInfos.Any())
                        Logger.Warn(x => x("MessageListeningManager:Couldnt find any public instance method called [{0}] in type [{1}] to handle message of type[{2}]", "HandleMessage", type.FullName, message.GetType()));
                    
                    var parameters = new[] {message};
                    foreach (var dispatchMethodInfo in dispatchMethodInfos)
                    {
                        TypesHelper.InvokeFast(dispatchMethodInfo, handler, parameters);
                        Logger.Trace(
                            x =>
                            x("MessageListeningManager: Invoked {0} passing argument {1}", dispatchMethodInfo.Name,
                              message));
                    }
                }
                else
                {
                    //if it doesnt exist, means its from previous sessions and then removes it
                    SuscriptionsDatasource.RemoveByHandlerId(suscriptionId);
                    Logger.Trace(x => x("MessageListeningManager: Removed subscriptionId {0}", suscriptionId));

                }
            }catch(Exception ex)
            {
                Logger.Error(ex);
                throw ex;
            }
        }

        //TODO: MOVE FROM HERE to the service register manager
        private void SavePublishedService(Type interfaceType, Type serviceImplementationType)
        {
            if (!interfaceType.IsInterface)
                throw new InvalidOperationException(string.Format("{0} must be an interface", interfaceType.FullName));

            if (!ServiceContractAttribute.IsDefinedIn(interfaceType))
                throw new InvalidOperationException(string.Format("{0} must be decorated with [ServiceCotract]",
                                                                  interfaceType.FullName));
            if (!serviceImplementationType.TypeImplements(interfaceType) || serviceImplementationType.IsAbstract)
                throw new ArgumentException(string.Format("Service implementation must implement {0}",
                                                          interfaceType.FullName));


            //TODO: IN SINGLE TRANSACTION

            #region todo in single transaction

            IList<ServiceDetails> svcList = ServiceDetailsDataSource.GetByInterfaceType(interfaceType);
            //SYSTEM SERVICES CAN BE DUPLICATED, BUSINESS SERVICES NO while there are return values, THE NEXT LINES ARE FINE WITH SMALL CHANGES
            if (svcList.Count > 0 && !svcList[0].IsSystemService &&
                svcList[0].Publisher != ComponentSettings.ComponentId &&
                svcList[0].ComponentOwner == ComponentSettings.ComponentId &&
                TypesHelper.GetPublicInstanceMethods(interfaceType).Any(x=>x.ReturnType!=typeof(void))
                )
                throw new InvalidOperationException(
                    string.Format("The service is already published by the component with Id:{0}."+
                    "{1}Only the services whose methods dont return values can be published by several components.", 
                    svcList[0].Publisher,Environment.NewLine));

            MethodInfo[] methods = TypesHelper.GetPublicInstanceMethods(interfaceType);

            foreach (MethodInfo method in methods) //join with next when overloaded
            {
                if (methods.Count(x => x.Name == method.Name) > 1)
                    throw new InvalidOperationException("Service registration: Method overload is not supported");
            }

            //ServiceDetailsDataSource.Remove(svcList);
            var isSystemService = ServiceContractAttribute.GetIsSystemService(interfaceType);
            foreach (MethodInfo method in methods) //TODO: ISSUE 22
            {
                if (Attribute.IsDefined(method, typeof (ServiceOperationAttribute)))
                {
                    var serviceDetails = new ServiceDetails
                                             {
                                                 ComponentOwner = ComponentSettings.ComponentId,
                                                 OperationIdentifier =
                                                     ServiceOperationAttribute.GetOperationIdentifier(
                                                         interfaceType, method.Name),
                                                 Publisher = ComponentSettings.ComponentId,
                                                 ServiceImplementationMethodName = method.Name,
                                                 ServiceInterfaceTypeName = interfaceType.FullName,
                                                 ServiceImplementationTypeName = serviceImplementationType.FullName,
                                                 IsSystemService = isSystemService
                                             };
                    ServiceDetailsDataSource.Save(serviceDetails);
                }
            }

            if (!isSystemService) //TODO: SYS SVCS ARE BINDED by module, they shouyld use the same code
            {
                var instance = ObjectBuilder.FromType<IService>(serviceImplementationType);
                IoCManager.Kernel.Bind(interfaceType).ToConstant(instance);
                //IoCManager.Kernel.Bind(interfaceType).To(serviceImplementationType).InSingletonScope();
            }

            #endregion
        }
        private void SaveSuscription(Type handlerInterfaceType, Guid suscriptionHandlerId, Type handlerType)
        {

            IncomingMessageSuscription incomingMessageSuscription =
                SuscriptionsDatasource.GetByHandlerId(suscriptionHandlerId);
            if (incomingMessageSuscription == null) //as it could exist from previous executions
            {
                if (!handlerInterfaceType.IsInterface ||
                    handlerInterfaceType.GetGenericTypeDefinition() != typeof (IHandleMessages<>))
                    throw new ArgumentException("handlerType must implement IHandleMessages");
                Type messageType = handlerInterfaceType.GetGenericArguments()[0];
                SuscriptionsDatasource.SaveIncommingSubscription(suscriptionHandlerId, handlerType, messageType);
            }
        }

        

        private void AddToHandlers(Guid suscriptionHandlerId, object handler)
        {
            if (_suscriptions.ContainsKey(suscriptionHandlerId))
                throw new InvalidOperationException(string.Format(
                    "The suscription with handler {0} was already bounded",
                    suscriptionHandlerId));

            _suscriptions.Add(suscriptionHandlerId, handler);
        }

        #region IDisposable

        private bool _disposed;


        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    //TODO: check that stop servers wont break in a multi-threaded client
                    ListeningManager.Dispose();
                }

                _disposed = true;
            }
        }

        #endregion
    }
}