using System;
using Stateless;

namespace ermeX.ComponentServices.ComponentSetup
{
	internal interface ISetupPayloader
	{
		void InjectServices();
		void RunUpgrades();
		
	}

	class SetupPayloader : ISetupPayloader
	{

		public void InjectServices()
		{
			throw new System.NotImplementedException();
		}

		public void RunUpgrades()
		{
			throw new System.NotImplementedException();
		}
	}
}