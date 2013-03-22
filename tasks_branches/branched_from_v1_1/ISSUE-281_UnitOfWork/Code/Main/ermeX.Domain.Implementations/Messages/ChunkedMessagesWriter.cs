using Ninject;
using ermeX.DAL.Interfaces;
using ermeX.Domain.Messages;
using ermeX.Transport.Interfaces.Messages;

namespace ermeX.Domain.Implementations.Messages
{
	class ChunkedMessagesWriter : ICanWriteChunkedMessages
	{
		private IChunkedServiceRequestMessageDataSource Repository { get; set; }

        [Inject]
		public ChunkedMessagesWriter(IChunkedServiceRequestMessageDataSource repository)
        {
            Repository = repository;
        }

		public void Save(ChunkedServiceRequestMessage chunk)
		{
			Repository.Save(chunk);
		}

		public void Remove(ChunkedServiceRequestMessage chunk)
		{
			Repository.Remove(chunk);
		}
	}
}