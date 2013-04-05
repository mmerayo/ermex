namespace ermeX.Domain.Observers
{
	public enum ObservableAction
	{
		Add,
		Update,
		Remove
	}
	//TODO: THIS TO BE REMOVED ONCE THE STATE MACHINE IS IN PLACE
	interface IDomainObserver<TModelInfo>
	{
		void Notify(ObservableAction action, TModelInfo entity);
	}
}