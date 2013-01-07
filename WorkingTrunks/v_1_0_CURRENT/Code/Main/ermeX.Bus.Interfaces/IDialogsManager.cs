// /*---------------------------------------------------------------------------------------*/
// If you viewing this code.....
// The current code is under construction.
// The reason you see this text is that lot of refactors/improvements have been identified and they will be implemented over the next iterations versions. 
// This is not a final product yet.
// /*---------------------------------------------------------------------------------------*/
using System;
using System.Collections.Generic;
using ermeX.Entities.Entities;
using ermeX.Interfaces;

namespace ermeX.Bus.Interfaces
{
    internal interface IDialogsManager
    {
        ///// <summary>
        /////   Request configured node(Anarquik) - nodes - service providers - message suscriptions
        ///// </summary>
        ///// <param name="componentId"> </param>
        //void JoinNetworkComponent(Guid componentId);

        void JoinNetwork();

        /// <summary>
        ///   Comunicates the subscription to every currently registered component
        /// </summary>
        /// <param name="subscriptionHandlerId"> </param>
        /// <param name="notifyComponents"> true notifies other components </param>
        void Suscribe(Guid subscriptionHandlerId);

        void NotifyService<TService>(Type serviceImplementationType) where TService : IService;
        void NotifyService(Type serviceInterface, Type serviceImplementation);
        void ExchangeDefinitions();
        void ExchangeDefinitions(AppComponent appComponent);

        void NotifyCurrentStatus();
        void UpdateRemoteServiceDefinition(string interfaceName, string methodName);
        void UpdateRemoteServiceDefinition(string interfaceName, string methodName, AppComponent appComponent);
        void EnsureDefinitionsAreExchanged(AppComponent appComponent, int retries=1);
        void EnsureDefinitionsAreExchanged(IEnumerable<AppComponent> appComponents,int retries=1);
    }
}