// /*---------------------------------------------------------------------------------------*/
// If you viewing this code.....
// The current code is under construction.
// The reason you see this text is that lot of refactors/improvements have been identified and they will be implemented over the next iterations versions. 
// This is not a final product yet.
// /*---------------------------------------------------------------------------------------*/
using System;
using System.Collections.Generic;
using NUnit.Framework;
using ermeX.ConfigurationManagement.Settings.Data.DbEngines;
using ermeX.Entities.Entities;
using ermeX.Tests.Acceptance.Dummy;
using ermeX.Tests.Common.DataAccess;
using ermeX.Tests.Common.SettingsProviders;


namespace ermeX.Tests.Acceptance
{
    //[TestFixture]
    internal abstract class AcceptanceTester:DataAccessTestBase
    {
        #region Setup/Teardown

        public override void OnStartUp()
        {
            CreateDatabase = false;
            base.OnStartUp();
        }

        public override void OnTearDown()
        {
           TestComponent.DisposeDomains();

           base.OnTearDown();            
        }

        public override void OnFixtureTearDown()
        {
            base.OnFixtureTearDown();
            TestSettingsProvider.DropDatabases();//TODO: REMOVE WHEN FIXED
        }

        #endregion

       

        protected void InitializeLonelyComponent(DbEngineType engineType, ushort listeningPort, TestComponent component, string dbConnString)
        {
            component.Start(engineType,dbConnString, listeningPort);
        }

        protected void InitializeConnectedComponent(DbEngineType engineType, ushort friendListeningPort, ushort localListeningPort, TestComponent friendComponent, string dbConnString, TestComponent component)
        {
            component.Start(engineType,dbConnString, localListeningPort, friendComponent.ComponentId, friendListeningPort);
        }

       
    }
}