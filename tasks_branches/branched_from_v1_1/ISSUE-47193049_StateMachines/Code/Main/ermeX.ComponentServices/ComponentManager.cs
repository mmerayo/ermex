using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Stateless;
using ermeX.ComponentServices.ComponentSetup;

namespace ermeX.ComponentServices
{
	internal sealed class ComponentManager
	{
		private readonly SetupMachine _setupMachine;
		public static readonly ComponentManager Default=new ComponentManager();
		private ComponentManager()
		{
			_setupMachine=new SetupMachine(null);
			_setupMachine.Setup();
		}
	}
}
