using System;
using ermeX.Entities.Entities;

namespace ermeX.DAL.Interfaces.Messages
{
    interface ICanReadChunkedMessages
    {
        ChunkedServiceRequestMessageData Fetch(Guid correlationId, int order);
    }
}
