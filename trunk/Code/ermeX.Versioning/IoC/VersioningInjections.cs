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

namespace ermeX.Versioning.IoC
{
    internal class VersioningInjections : NinjectModule
    {

        public VersioningInjections(IDalSettings settings)
        {
        }

        public override void Load()
        {
            Bind<IVersionUpgradeHelper>().To<VersionUpgradeHelper>().InSingletonScope();
        }
    }
}