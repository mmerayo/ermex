// /*---------------------------------------------------------------------------------------*/
// If you viewing this code.....
// The current code is under construction.
// The reason you see this text is that lot of refactors/improvements have been identified and they will be implemented over the next iterations versions. 
// This is not a final product yet.
// /*---------------------------------------------------------------------------------------*/
using System;
using NUnit.Framework;
using ermeX.ConfigurationManagement.Settings.Data.DbEngines;
using ermeX.DAL.DataAccess.DataSources;
using ermeX.Entities.Base;
using ermeX.Tests.Common.SettingsProviders;

namespace ermeX.Tests.DAL.Integration.DataSources
{
    /// <summary>
    /// inherit from this if the entity can be updated by external components
    /// </summary>
    /// <typeparam name="TDataSource"></typeparam>
    /// <typeparam name="TModel"></typeparam>
    internal abstract class UpdatableByExternalComponentsTester<TDataSource, TModel> :
        DataSourceTesterBase<TDataSource, TModel>
        where TDataSource : DataSource<TModel>
        where TModel : ModelBase, new()
    {
        [Test, TestCaseSource(typeof(TestCaseSources), "AllDbs")]
        public void Doesnt_Update_When_Version_Is_Older_Than_Current(DbEngineType engine)
        {
            int id = InsertRecord(engine);
            TDataSource target = GetDataSourceTarget(engine);
            TModel expected = target.GetById(id);
            Assert.IsTrue(expected.Version != DateTime.MinValue.Ticks);

            expected = GetExpectedWithChanges(expected);

            target.Save(expected);

            long expVersion = expected.Version;
            expected.Version--;

            target.Save(expected);
            var actual = GetDataHelper(engine).QueryTestHelper.GetObjectFromRow<TModel>(GetByIdSqlQuery(expected));

            Assert.AreEqual(expVersion.ToString(), actual.Version.ToString());
        }

       
    }
}