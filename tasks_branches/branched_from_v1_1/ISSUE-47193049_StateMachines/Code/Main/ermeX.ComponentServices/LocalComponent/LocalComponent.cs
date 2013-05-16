using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Common.Logging;
using Stateless;

namespace ermeX.ComponentServices.LocalComponent
{
	internal sealed partial class LocalComponent:ILocalComponent
	{
		private static readonly ILog Logger = LogManager.GetLogger<LocalComponent>();

		public LocalComponent()
		{
			DefineStateMachineTransitions();
		}

		
	}
}
