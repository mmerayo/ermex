using System.Collections.Generic;
using System.Linq;
using System.Text;
using ermeX.ConfigurationManagement.Settings.Data.DbEngines;
using ermeX.DAL.Interfaces;

namespace ermeX.Domain.QueryDatabase
{
	internal interface IQueryHelperFactory
	{
		IQueryHelper GetHelper(DbEngineType type, string connectionString);
	}
}
