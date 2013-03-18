using System;
using ermeX.Entities.Entities;

namespace ermeX.Domain.ChunkedMessages
{
    interface ICanGetChunkedMessages
    {
        ChunkedServiceRequestMessageData Fetch(Guid correlationId, int order);
    }
}
