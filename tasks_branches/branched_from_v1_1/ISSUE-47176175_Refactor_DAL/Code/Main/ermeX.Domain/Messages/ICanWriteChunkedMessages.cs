using ermeX.Transport.Interfaces.Messages;

namespace ermeX.Domain.Messages
{
	interface ICanWriteChunkedMessages
	{
		void Save(ChunkedServiceRequestMessage message); //TODO: THE CHUNKED MESSAGES TO NOT TO EXIST
		void Remove(ChunkedServiceRequestMessage chunk);
	}
}