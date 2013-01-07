// /*---------------------------------------------------------------------------------------*/
// If you viewing this code.....
// The current code is under construction.
// The reason you see this text is that lot of refactors/improvements have been identified and they will be implemented over the next iterations versions. 
// This is not a final product yet.
// /*---------------------------------------------------------------------------------------*/

using System;
using System.Collections.Generic;
using System.Configuration;
using Ninject.Modules;
using ermeX.ConfigurationManagement.Settings;
using ermeX.DAL.DataAccess.DataSources;
using ermeX.DAL.DataAccess.Helpers;
using ermeX.DAL.DataAccess.Providers;
using ermeX.DAL.Interfaces;

namespace ermeX.DAL.IoC
{
    internal class DALInjections : NinjectModule
    {
         private readonly IDalSettings _settings;

         public DALInjections(IDalSettings settings)
        {
            if (settings == null) throw new ArgumentNullException("settings");

            List<string> errors;
            if (!new DalSettingsValidator(settings).Validate(out errors))
            {
                throw new ConfigurationException(string.Join(",", errors));
            }
            _settings = settings;
        }

        public override void Load()
        {
            //TODO: create interfaces and bind

            Bind<IAppComponentDataSource>().To<AppComponentDataSource>().InSingletonScope();
            Bind<IConnectivityDetailsDataSource>().To<ConnectivityDetailsDataSource>().InSingletonScope();
            Bind<IOutgoingMessageSuscriptionsDataSource>().To<OutgoingMessageSuscriptionsDataSource>().InSingletonScope();
            Bind<IIncomingMessageSuscriptionsDataSource>().To<IncomingMessageSuscriptionsDataSource>().InSingletonScope();
            Bind<IOutgoingMessagesDataSource>().To<OutgoingMessagesDataSource>().InSingletonScope();
            Bind<IIncomingMessagesDataSource>().To<IncomingMessagesDataSource>().InSingletonScope();
            Bind<IServiceDetailsDataSource>().To<ServiceDetailsDataSource>().InSingletonScope();
            Bind<IChunkedServiceRequestMessageDataSource>().To<ChunkedServiceRequestMessageDataSource>().InSingletonScope();
            Bind<IBusMessageDataSource>().To<BusMessageDataSource>().InSingletonScope();
            
            Bind<IAutoRegistration>().To<AutoRegistration>().InSingletonScope();
            Bind<IDataAccessExecutor>().To<DataAccessExecutor>().InSingletonScope();


        }
    }
}