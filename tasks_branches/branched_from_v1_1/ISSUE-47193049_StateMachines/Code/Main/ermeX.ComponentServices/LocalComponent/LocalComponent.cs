using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Common.Logging;
using Stateless;

namespace ermeX.ComponentServices.LocalComponent
{
	internal sealed class LocalComponent:ILocalComponent,ILocalStateMachinePayloader
	{
		private static readonly ILog Logger = LogManager.GetLogger<LocalComponent>();
		private readonly LocalComponentStateMachine _stateMachine;

		public LocalComponent()
		{
			_stateMachine = new LocalComponentStateMachine(this);
		}


		public void Reset()
		{
			Logger.Debug("Reset");
			throw new NotImplementedException();
		}

		public void Stop()
		{
			Logger.Debug("Stop");
			throw new NotImplementedException();
		}

		public void Run()
		{
			Logger.Debug("Run");
			throw new NotImplementedException();
		}

		public void PublishServices()
		{
			Logger.Debug("PublishServices");
			throw new NotImplementedException();
		}

		public void SubscribeToMessages()
		{
			Logger.Debug("SubscribeToMessages");
			throw new NotImplementedException();
		}

		public void Start()
		{
			Logger.Debug("Start");
			_stateMachine.Start();
		}

		public void Error()
		{
			Logger.Debug("Error");
			throw new NotImplementedException();
		}

		
	}
}
