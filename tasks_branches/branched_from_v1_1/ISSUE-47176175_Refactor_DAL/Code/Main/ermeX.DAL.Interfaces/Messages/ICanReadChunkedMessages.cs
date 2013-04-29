using System;
using ermeX.Models.Entities;

namespace ermeX.DAL.Interfaces.Messages
{
    interface ICanReadChunkedMessages
    {
        ChunkedServiceRequestMessageData Fetch(Guid correlationId, int order);
    }
}
