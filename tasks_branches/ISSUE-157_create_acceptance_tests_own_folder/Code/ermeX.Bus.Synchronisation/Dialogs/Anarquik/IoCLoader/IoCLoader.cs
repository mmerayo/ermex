// /*---------------------------------------------------------------------------------------*/
// If you viewing this code.....
// The current code is under construction.
// The reason you see this text is that lot of refactors/improvements have been identified and they will be implemented over the next iterations versions. 
// This is not a final product yet.
// /*---------------------------------------------------------------------------------------*/
using ermeX.Bus.Synchronisation.DependencyInjectionModules;
using ermeX.Bus.Synchronisation.Dialogs.Anarquik.HandledByMessageQueue;
using ermeX.Bus.Synchronisation.Dialogs.Anarquik.HandledByService;
using ermeX.Bus.Synchronisation.Dialogs.HandledByMessageQueue;
using ermeX.Bus.Synchronisation.Dialogs.HandledByService;

namespace ermeX.Bus.Synchronisation.Dialogs.Anarquik.IoCLoader
{
    internal static class IoCLoader
    {
        public static void PerformInjections(DialogInjections injector)
        {
            injector.Bind<IHandshakeService>().To<HandshakeServiceHandler>().InSingletonScope();
            injector.Bind<IMessageSuscriptionsService>().To<MessageSuscriptionsRequestMessageHandler>().InSingletonScope
                ();
            injector.Bind<IPublishedServicesDefinitionsService>().To<PublishedServicesHandler>().InSingletonScope();
            injector.Bind<IUpdatePublishedServiceMessageHandler>().To<UpdatePublishedServiceMessageHandler>().
                InSingletonScope();
            injector.Bind<IUpdateSuscriptionMessageHandler>().To<UpdateSuscriptionMessageHandler>().InSingletonScope();
        }
    }
}