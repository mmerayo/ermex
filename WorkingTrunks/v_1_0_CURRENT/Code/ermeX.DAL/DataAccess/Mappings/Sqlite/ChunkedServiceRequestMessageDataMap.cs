// /*---------------------------------------------------------------------------------------*/
// If you viewing this code.....
// The current code is under construction.
// The reason you see this text is that lot of refactors/improvements have been identified and they will be implemented over the next iterations versions. 
// This is not a final product yet.
// /*---------------------------------------------------------------------------------------*/

using ermeX.ConfigurationManagement.Settings.Data.DbEngines;

namespace ermeX.DAL.DataAccess.Mappings.Sqlite

{
    internal class ChunkedServiceRequestMessageDataMap : Mappings.ChunkedServiceRequestMessageDataMap
    {
        protected override DbEngineType EngineType
        {
            get { return DbEngineType.Sqlite; }
        }
    }
}