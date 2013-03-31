using System;
using System.Collections.Generic;
using Ninject;
using ermeX.DAL.DataAccess.DataSources;
using ermeX.DAL.Interfaces;
using ermeX.DAL.Interfaces.Observer;
using ermeX.Domain.Observers;
using ermeX.Entities.Entities;
using ermeX.Threading.Queues;

namespace ermeX.Domain.Implementations.Observers
{

	internal sealed class DomainNotifier : IDomainObservable,
	                                                               IDalObserver<OutgoingMessageSuscription>
	{
		private readonly IDalObservable<OutgoingMessageSuscription> _outgoingMessageSuscriptionsDataSource;
		private readonly IList<Type> _supportedModelTypes = new[] {typeof (OutgoingMessageSuscription)};

		private Dictionary<Type, List<object>> _subscribers = new Dictionary<Type, List<object>>();

		[Inject]
		public DomainNotifier(
			IOutgoingMessageSuscriptionsDataSource outgoingMessageSuscriptionsDataSource)
		{
			_outgoingMessageSuscriptionsDataSource = outgoingMessageSuscriptionsDataSource;


		}

		public void AddObserver<TModelInfo>(IDomainObserver<TModelInfo> observer)
		{
			if (observer == null) throw new ArgumentNullException("observer");

			Type modelType = typeof (TModelInfo);
			if (!_supportedModelTypes.Contains(modelType))
				throw new NotImplementedException(string.Format("The subscription to {0} has not been implemented.",
				                                                modelType.FullName));

			lock (this)
			{
				if (!_subscribers.ContainsKey(modelType))
				{
					_subscribers.Add(modelType, new List<object>());
					_outgoingMessageSuscriptionsDataSource.AddObserver(this);
				}
				_subscribers[modelType].Add(observer);
			}
		}

		public void RemoveObserver<TModelInfo>(IDomainObserver<TModelInfo> observer)
		{
			if (observer == null) throw new ArgumentNullException("observer");

			Type modelType = typeof (TModelInfo);
			lock (this)
				_subscribers[modelType].Remove(observer);
		}

		public void Notify(NotifiableDalAction action, OutgoingMessageSuscription entity)
		{
			lock (this)
			{
				var subscribers = _subscribers[typeof (OutgoingMessageSuscription)];
				if (subscribers.Count == 0)
					return;
				ObservableAction observableAction;
				switch (action)
				{
					case NotifiableDalAction.Add:
						observableAction = ObservableAction.Add;
						break;
					case NotifiableDalAction.Update:
						observableAction = ObservableAction.Update;
						break;
					case NotifiableDalAction.Remove:
						observableAction = ObservableAction.Remove;
						break;
					default:
						throw new ArgumentOutOfRangeException("action");
				}

				foreach (var subscriber in subscribers)
				{
					SystemTaskQueue.Instance.EnqueueItem(
						() =>
							{
								((IDomainObserver<OutgoingMessageSuscription>) subscriber).Notify(observableAction, entity);
							});
				}
			}
		}
	}
}