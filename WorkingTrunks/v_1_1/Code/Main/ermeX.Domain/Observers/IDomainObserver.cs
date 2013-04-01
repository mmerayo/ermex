namespace ermeX.Domain.Observers
{
	public enum ObservableAction
	{
		Add,
		Update,
		Remove
	}

	interface IDomainObserver<TModelInfo>
	{
		void Notify(ObservableAction action, TModelInfo entity);
	}
}