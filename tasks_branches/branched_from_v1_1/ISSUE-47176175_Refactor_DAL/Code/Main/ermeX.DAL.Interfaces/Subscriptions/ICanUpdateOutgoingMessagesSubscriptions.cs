using ermeX.Entities.Entities;

namespace ermeX.DAL.Interfaces.Subscriptions
{
	internal interface ICanUpdateOutgoingMessagesSubscriptions
	{
		/// <summary>
		/// Saves an outgoing subscription from an incomming suscription in other component
		/// </summary>
		void ImportFromOtherComponent(IncomingMessageSuscription susbcription);

		void ImportFromOtherComponent(OutgoingMessageSuscription susbcription);
	}
}