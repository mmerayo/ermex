// /*---------------------------------------------------------------------------------------*/
// If you viewing this code.....
// The current code is under construction.
// The reason you see this text is that lot of refactors/improvements have been identified and they will be implemented over the next iterations versions. 
// This is not a final product yet.
// /*---------------------------------------------------------------------------------------*/
using System;
using Ninject.Modules;
using ermeX.Bus.Interfaces;
using ermeX.Bus.Synchronisation.Dialogs.Anarquik.IoCLoader;
using ermeX.ConfigurationManagement.Settings;
using ermeX.ConfigurationManagement.Settings.Component;

namespace ermeX.Bus.Synchronisation.DependencyInjectionModules
{
    internal class DialogInjections : NinjectModule
    {
        private readonly IBusSettings _settings;

        public DialogInjections(IBusSettings settings)
        {
            if (settings == null) throw new ArgumentNullException("settings");
            _settings = settings;
        }

        public override void Load()
        {
            Bind<IDialogsManager>().To<DialogsManager>().InSingletonScope();

            //TODO: DIALOGS FOR ANARQUIK OR GOVERNED

            switch (_settings.NetworkingMode)
            {
                case NetworkingMode.Anarquik:
                    IoCLoader.PerformInjections(this);
                    break;
                case NetworkingMode.Governed:
                    throw new NotSupportedException(
                        "The Governed networking mode is not supported yet. Keep an eye on the updates");
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}