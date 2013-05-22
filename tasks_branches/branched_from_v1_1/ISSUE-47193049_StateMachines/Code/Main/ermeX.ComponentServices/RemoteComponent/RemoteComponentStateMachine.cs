using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ermeX.ComponentServices.RemoteComponent
{
	internal sealed class RemoteComponentStateMachine:IRemoteComponentStateMachine
	{
		private enum RemoteComponentEvent
		{
			Create=0,
			Ready,
			Join,
			Joined,
			ToError,
			Stop
		}
		private enum RemoteComponentState
		{
			Prelive=0,
			Created,
			Stopped,
			Joining,
			Joined,
			Errored
		}


		public void Create()
		{
			throw new NotImplementedException();
		}

		public void Join()
		{
			throw new NotImplementedException();
		}

		public void Stop()
		{
			throw new NotImplementedException();
		}

		public bool IsErrored()
		{
			throw new NotImplementedException();
		}

		public bool IsStopped()
		{
			throw new NotImplementedException();
		}

		public bool IsRunning()
		{
			throw new NotImplementedException();
		}
	}
}
