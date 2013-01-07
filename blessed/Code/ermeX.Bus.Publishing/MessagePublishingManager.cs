// /*---------------------------------------------------------------------------------------*/
// If you viewing this code.....
// The current code is under construction.
// The reason you see this text is that lot of refactors/improvements have been identified and they will be implemented over the next iterations versions. 
// This is not a final product yet.
// /*---------------------------------------------------------------------------------------*/
using System;
using System.Collections.Generic;
using Castle.DynamicProxy;
using Ninject;
using ermeX.Bus.Interfaces;
using ermeX.Common;
using ermeX.ConfigurationManagement.IoC;
using ermeX.DAL.Interfaces;
using ermeX.Entities.Entities;
using ermeX.Interfaces;
using ermeX.LayerMessages;

namespace ermeX.Bus.Publishing
{
    internal class MessagePublishingManager : BusInteropBase, IMessagePublisher
    {
        [Inject]
        public MessagePublishingManager(IEsbManager bus, IServiceDetailsDataSource serviceDetailsDataSource) : base(bus)
        {
            ServiceDetailsDataSource = serviceDetailsDataSource;
            if (serviceDetailsDataSource == null) throw new ArgumentNullException("serviceDetailsDataSource");
        }

        private IServiceDetailsDataSource ServiceDetailsDataSource { get; set; }

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
            IList<ServiceDetails> operations = ServiceDetailsDataSource.GetByInterfaceType(typeof (TServiceInterface));
            if (operations.Count == 0)
                return default(TServiceInterface);

            //TODO: REPLACE in the cases THERE ARE REAL local implementations instead of the PROXIES
            var proxy = IoCManager.Kernel.Get<IServiceCallsProxy>();

            if (!componentId.IsEmpty())
                proxy.SetDestinationComponent(componentId);

            var obj = ObjectBuilder.CreateProxy<TServiceInterface>((IInterceptor) proxy);
            return obj;
        }

        #endregion
    }
}