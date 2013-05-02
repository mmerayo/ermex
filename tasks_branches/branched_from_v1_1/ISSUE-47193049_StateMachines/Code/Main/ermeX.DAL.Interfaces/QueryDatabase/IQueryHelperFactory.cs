using ermeX.ConfigurationManagement.Settings.Data.DbEngines;

namespace ermeX.DAL.Interfaces.QueryDatabase
{
	internal interface IQueryHelperFactory
	{
		IQueryHelper GetHelper(DbEngineType type, string connectionString);
	}
}
