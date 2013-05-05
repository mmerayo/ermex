using System;
using Stateless;

namespace ermeX.ComponentServices.ComponentSetup
{
	internal interface ISetupPayloader
	{
		void InjectServices(StateMachine<SetupMachine.SetupProcessState, SetupMachine.SetupEvent>.Transition o);
		void RunUpgrades(StateMachine<SetupMachine.SetupProcessState, SetupMachine.SetupEvent>.Transition o);
		void HandleError(Exception ex);
		
		void SetMachine(StateMachine<SetupMachine.SetupProcessState, SetupMachine.SetupEvent> machine);
		void TryFire(SetupMachine.SetupEvent e);
	}

	class SetupPayloader : ISetupPayloader
	{


		private SetupMachine _machine;
		

		public void InjectServices(StateMachine<SetupMachine.SetupProcessState, SetupMachine.SetupEvent>.Transition o)
		{
			try
			{
			}
			catch(Exception ex)
			{
				_machine.Fire(SetupMachine.SetupEvent.Error,ex.ToString());
			}
			TryFire(SetupMachine.SetupEvent.Injected);
		}

		public void RunUpgrades(StateMachine<SetupMachine.SetupProcessState, SetupMachine.SetupEvent>.Transition o)
		{
			throw new System.NotImplementedException();
		}

		public void HandleError(Exception ex)
		{
			if (ex == null) throw new ArgumentNullException("ex","Use FireError from machine");

			//TODO: LOG??
			throw ex;
		}


		public void SetMachine(SetupMachine machine)
		{
			_machine = machine;
		}

		public void TryFire(SetupMachine.SetupEvent e)
		{
			if (_machine==null)
				throw new InvalidOperationException("Invoke SetMachine first");
			if(!_machine.CanFire(e))
				throw new InvalidOperationException(string.Format("Cannot transit from {0}",_machine.State));
			_machine.Fire(e);
		}

		
	}
}