using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ermeX.Logging
{
	internal interface ILogManager
	{
		ILogger GetLogger<TType>();
		ILogger GetLogger(Type type);
	}
}
