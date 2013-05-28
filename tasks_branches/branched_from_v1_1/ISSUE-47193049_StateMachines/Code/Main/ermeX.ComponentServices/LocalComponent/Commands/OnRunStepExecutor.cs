using ermeX.ComponentServices.Interfaces.LocalComponent.Commands;

namespace ermeX.ComponentServices.LocalComponent.Commands
{
	class OnRunStepExecutor : IOnRunStepExecutor
	{
		public void Run()
		{
			var friendComponent = ComponentManager.Default.FriendComponent;
			if(friendComponent!=null)
				friendComponent.Join();
		}
	}
}