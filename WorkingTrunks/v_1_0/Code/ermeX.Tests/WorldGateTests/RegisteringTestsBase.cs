// /*---------------------------------------------------------------------------------------*/
// If you viewing this code.....
// The current code is under construction.
// The reason you see this text is that lot of refactors/improvements have been identified and they will be implemented over the next iterations versions. 
// This is not a final product yet.
// /*---------------------------------------------------------------------------------------*/
using System;
using System.Collections.Generic;
using ermeX.ConfigurationManagement.Settings.Data.DbEngines;
using ermeX.ConfigurationManagement.Settings.Data.Schemas;
using ermeX.Tests.Common.DataAccess;

namespace ermeX.Tests.WorldGateTests
{
    internal abstract class RegisteringTestsBase:DataAccessTestBase
    {
        public override void OnStartUp()
        {
            CreateDatabase = false;
            base.OnStartUp();
        }

        public override void OnTearDown()
        {
            WorldGate.Reset();
            base.OnTearDown();
        }
        
    }
}