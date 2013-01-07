// /*---------------------------------------------------------------------------------------*/
// If you viewing this code.....
// The current code is under construction.
// The reason you see this text is that lot of refactors/improvements have been identified and they will be implemented over the next iterations versions. 
// This is not a final product yet.
// /*---------------------------------------------------------------------------------------*/
using System.Collections.Generic;
using ermeX.Bus.Interfaces.Attributes;
using ermeX.Bus.Synchronisation.Messages;
using ermeX.Entities.Entities;
using ermeX.Interfaces;

namespace ermeX.Bus.Synchronisation.Dialogs.HandledByService
{
    [ServiceContract("06A43D33-79EC-428B-8327-82471BF8AEAF", true)]
    internal interface IPublishedServicesDefinitionsService : IService
    {
        /// <summary>
        ///   get all the services in the servers component
        /// </summary>
        /// <param name="request"> </param>
        /// <returns> </returns>
        [ServiceOperation("8551201E-F2C3-4C7C-8473-3372F97BD6C6")]
        PublishedServicesResponseMessage RequestDefinitions(PublishedServicesRequestMessage request);

        /// <summary>
        ///   Adds the services to the component
        /// </summary>
        /// <param name="service"> </param>
        [ServiceOperation("4485F944-4590-48A5-B69C-B072B62D3A0A")]
        void AddService(ServiceDetails service);

        /// <summary>
        ///   Adds the services to the component
        /// </summary>
        /// <param name="services"> </param>
        [ServiceOperation("095D0266-C707-4770-9F1E-F4BE60921A04")]
        void AddServices(IList<ServiceDetails> services);
    }
}