// /*---------------------------------------------------------------------------------------*/
// If you viewing this code.....
// The current code is under construction.
// The reason you see this text is that lot of refactors/improvements have been identified and they will be implemented over the next iterations versions. 
// This is not a final product yet.
// /*---------------------------------------------------------------------------------------*/
using System;
using System.Collections.Generic;
using NUnit.Framework;
using ermeX.ConfigurationManagement.Settings;
using ermeX.ConfigurationManagement.Settings.Data.DbEngines;
using ermeX.DAL.DataAccess.Helpers;
using ermeX.Tests.Common.DataAccess;
using ermeX.Tests.Common.SettingsProviders;

using ermeX.Versioning;

namespace ermeX.Tests.VersionUpgrade
{
    [Category(TestCategories.CoreFunctionalTest)]
    //[TestFixture]
    internal sealed class VersionUpgradeHelperTests :DataAccessTestBase
    {
       
        [Test, TestCaseSource(typeof(TestCaseSources), "AllDbs")]
        public void Can_RunUpgrades(DbEngineType engine)
        {
            DataAccessTestHelper dataAccessTestHelper = GetDataHelper(engine);
            IDalSettings settingsSource = dataAccessTestHelper.DataAccessSettings;
            QueryHelper queryTestHelper = QueryHelper.GetHelper(engine,
                                                                settingsSource.ConfigurationConnectionString);
            queryTestHelper.DeleteDbDefinitions();

            var target = new VersionUpgradeHelper();
            target.RunDataSchemaUpgrades(settingsSource.SchemasApplied,settingsSource.ConfigurationConnectionString,settingsSource.ConfigurationSourceType);

            Assert.IsTrue(queryTestHelper.MainDbDefinitionsExist(settingsSource),
                          "The expected db definitions were not created");
        }

        [Ignore]
        [Test, TestCaseSource(typeof(TestCaseSources), "AllDbs")]
        public void RunUpgrades_On_StartUp(DbEngineType engine)
        {
        }
    }
}