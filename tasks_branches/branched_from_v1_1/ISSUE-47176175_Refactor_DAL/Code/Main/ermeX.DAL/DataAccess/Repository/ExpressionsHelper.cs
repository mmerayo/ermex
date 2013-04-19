using System;
using System.Linq.Expressions;
using Common.Logging;
using ermeX.DAL.DataAccess.Providers;
using ermeX.Entities.Entities;

namespace ermeX.DAL.DataAccess.Repository
{
	//TODO: TEST && needs to be optimized
	internal class ExpressionsHelper : IExpressionHelper<AppComponent>,
	                                     IExpressionHelper<ChunkedServiceRequestMessageData>,
	                                     IExpressionHelper<ConnectivityDetails>,
	                                     IExpressionHelper<IncomingMessage>,
	                                     IExpressionHelper<IncomingMessageSuscription>,
	                                     IExpressionHelper<OutgoingMessage>,
	                                     IExpressionHelper<OutgoingMessageSuscription>,
	                                     IExpressionHelper<ServiceDetails>
	{

		public Expression<Func<AppComponent, bool>> GetFindByBizKey(AppComponent e)
		{
			return x => (x).ComponentOwner == e.ComponentOwner
			            && (x).ComponentId == e.ComponentId;
		}

		public Expression<Func<ChunkedServiceRequestMessageData, bool>> GetFindByBizKey(ChunkedServiceRequestMessageData e)
		{
			return x =>
			       (x).ComponentOwner == e.ComponentOwner
			       && (x).CorrelationId == e.CorrelationId
			       && (x).Order == e.Order;
		}

		public Expression<Func<ConnectivityDetails, bool>> GetFindByBizKey(ConnectivityDetails e)
		{
			return x => (x).ComponentOwner == e.ComponentOwner 
				 && x.ServerId==e.ServerId;
						//&& (x).Ip == e.Ip
						//&& (x).Port == e.Port;
		}

		public Expression<Func<IncomingMessage, bool>> GetFindByBizKey(IncomingMessage e)
		{
			return x => x.ComponentOwner == e.ComponentOwner
						&& x.SuscriptionHandlerId==e.SuscriptionHandlerId
						&& x.Status==e.Status
			            && x.MessageId == e.MessageId
			            && x.PublishedBy == e.PublishedBy
			            && x.PublishedTo == e.PublishedTo;
		}

		public Expression<Func<IncomingMessageSuscription, bool>> GetFindByBizKey(IncomingMessageSuscription e)
		{
			return x => (x).ComponentOwner == e.ComponentOwner
			            && x.BizMessageFullTypeName == e.BizMessageFullTypeName
			            && x.SuscriptionHandlerId == e.SuscriptionHandlerId
			            && x.HandlerType == e.HandlerType;
		}

		public Expression<Func<OutgoingMessage, bool>> GetFindByBizKey(OutgoingMessage e)
		{
			return x => x.ComponentOwner == e.ComponentOwner
			            && x.MessageId == e.MessageId
						&& x.PublishedTo==e.PublishedTo
						&& x.PublishedBy==e.PublishedBy;
		}

		public Expression<Func<OutgoingMessageSuscription, bool>> GetFindByBizKey(OutgoingMessageSuscription e)
		{
			return x => (x).ComponentOwner == e.ComponentOwner
			            && (x).BizMessageFullTypeName == e.BizMessageFullTypeName
			            && (x).Component == e.Component;
		}

		public Expression<Func<ServiceDetails, bool>> GetFindByBizKey(ServiceDetails e)
		{
			return x => (x).ComponentOwner == e.ComponentOwner
			            && x.OperationIdentifier == e.OperationIdentifier
			            && (x).ServiceInterfaceTypeName == e.ServiceInterfaceTypeName
			            && (x).ServiceImplementationMethodName == e.ServiceImplementationMethodName
			            && (x).Publisher == e.Publisher;
		}
	}
}