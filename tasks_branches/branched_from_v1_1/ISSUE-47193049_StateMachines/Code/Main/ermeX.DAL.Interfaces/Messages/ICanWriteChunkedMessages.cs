using ermeX.Transport.Interfaces.Messages;

namespace ermeX.DAL.Interfaces.Messages
{
	interface ICanWriteChunkedMessages
	{
		void Save(ChunkedServiceRequestMessage message); //TODO: THE CHUNKED MESSAGES TO NOT TO EXIST
		void Remove(ChunkedServiceRequestMessage chunk);
	}
}