namespace ermeX.ComponentServices.ComponentSetup
{
	internal interface ISetupPayloader
	{
		void InjectServices(object o);
		void RunUpgrades(object o);
		void HandleError(object o);
	}
}