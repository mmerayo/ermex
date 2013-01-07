// /*---------------------------------------------------------------------------------------*/
// If you viewing this code.....
// The current code is under construction.
// The reason you see this text is that lot of refactors/improvements have been identified and they will be implemented over the next iterations versions. 
// This is not a final product yet.
// /*---------------------------------------------------------------------------------------*/

using System.Collections.Generic;
using ermeX.Entities.Entities;
using ermeX.LayerMessages;

namespace ermeX.DAL.Interfaces
{
    internal interface IBusMessageDataSource : IDataSource<BusMessageData>
    {
        IList<BusMessageData> GetMessagesToDispatch();
    }
}