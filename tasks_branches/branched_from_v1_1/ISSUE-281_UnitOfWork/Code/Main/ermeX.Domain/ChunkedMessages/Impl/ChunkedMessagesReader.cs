using System;
using Ninject;
using ermeX.DAL.Interfaces;
using ermeX.Entities.Entities;

namespace ermeX.Domain.ChunkedMessages.Impl
{
    internal sealed class ChunkedMessagesReader : ICanGetChunkedMessages
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