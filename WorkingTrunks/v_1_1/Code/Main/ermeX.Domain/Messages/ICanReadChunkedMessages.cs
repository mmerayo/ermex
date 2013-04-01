using System;
using ermeX.Entities.Entities;

namespace ermeX.Domain.Messages
{
    interface ICanReadChunkedMessages
    {
        ChunkedServiceRequestMessageData Fetch(Guid correlationId, int order);
    }
}
