// /*---------------------------------------------------------------------------------------*/
// If you viewing this code.....
// The current code is under construction.
// The reason you see this text is that lot of refactors/improvements have been identified and they will be implemented over the next iterations versions. 
// This is not a final product yet.
// /*---------------------------------------------------------------------------------------*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ermeX.ConfigurationManagement.Settings.Data.DbEngines;

namespace ermeX.Tests.Common.SettingsProviders
{
    class TestCaseSources
    {
        public static IEnumerable<DbEngineType> InMemoryDb()
        {
            yield return DbEngineType.SqliteInMemory;
        }

        public static IEnumerable<DbEngineType> AllDbs()
        {
            Array enumValues = typeof (DbEngineType).GetEnumValues();
            return enumValues.Cast<DbEngineType>();
        }
        public static IEnumerable<object[]> InMemoryDbWithBool()
        {
            yield return new object[] {DbEngineType.SqliteInMemory, true};
            yield return new object[] {DbEngineType.SqliteInMemory, false};
        }

    }
}
