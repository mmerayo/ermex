using System;
using Ninject;
using ermeX.DAL.Interfaces;
using ermeX.Domain.Messages;
using ermeX.Entities.Entities;

namespace ermeX.Domain.Implementations.Messages
{
    internal sealed class ChunkedMessagesReader : ICanReadChunkedMessages
    {
        private IChunkedServiceRequestMessageDataSource Repository { get; set; }

        [Inject]
        public ChunkedMessagesReader(IChunkedServiceRequestMessageDataSource repository)
        {
            Repository = repository;
        }

        public ChunkedServiceRequestMessageData Fetch(Guid correlationId, int order)
        {
            return Repository.GetByCorrelationIdAndOrder(correlationId, order);//TODO: MOVE THE LOGIC HERE
        }
    }
}